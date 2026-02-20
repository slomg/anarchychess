using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.AnarchyBot.Errors;
using AnarchyChess.Api.AnarchyBot.Models;
using AnarchyChess.Api.AnarchyBot.Services;
using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;
using ErrorOr;

namespace AnarchyChess.Api.AnarchyBot.Grains;

[Alias("AnarchyChess.Api.AnarchyBot.Grains.IAnarchyBotGrain")]
public interface IAnarchyBotGrain : IGrainWithStringKey
{
    [Alias("CreateAsync")]
    Task StartGameAsync(GamePlayer player, CancellationToken token = default);

    [Alias("GetStateAsync")]
    Task<ErrorOr<AnarchyBotGameState>> GetStateAsync(CancellationToken token = default);

    [Alias("PlayMoveAsync")]
    Task<ErrorOr<Success>> PlayMoveAsync(
        UserId userId,
        MoveKey moveKey,
        CancellationToken token = default
    );

    [Alias("ResignAsync")]
    Task<ErrorOr<Success>> ResignAsync(UserId userId, CancellationToken token = default);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.AnarchyBot.Grains.AnarchyBotGameData")]
public class AnarchyBotGameData
{
    [Id(0)]
    public required PlayerRoster Players { get; init; }

    [Id(1)]
    public required GameColor HumanColor { get; init; }

    [Id(2)]
    public required GameColor BotColor { get; init; }

    [Id(3)]
    public required string InitialFen { get; init; }

    [Id(4)]
    public required GameCoreState Core { get; init; }

    [Id(5)]
    public MoveHistory MoveHistory { get; init; } = new();

    [Id(6)]
    public GameResultData? Result { get; set; }
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.AnarchyBot.Grains.AnarchyBotGrainState")]
public class AnarchyBotGrainState
{
    [Id(0)]
    public AnarchyBotGameData? CurrentGame { get; set; }
}

public class AnarchyBotGrain : Grain, IAnarchyBotGrain
{
    public const string StateName = "anarchyBotGame";

    private readonly ILogger<AnarchyBotGrain> _logger;
    private readonly IPersistentState<AnarchyBotGrainState> _state;
    private readonly IGameCore _core;
    private readonly IAnarchyBotService _anarchyBotService;
    private readonly IAnarchyBotNotifier _notifier;
    private readonly IGameArchiveService _gameArchiveService;
    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GameToken _gameToken;

    public AnarchyBotGrain(
        ILogger<AnarchyBotGrain> logger,
        [PersistentState(StateName)] IPersistentState<AnarchyBotGrainState> state,
        IGameCore core,
        IAnarchyBotService anarchyBotService,
        IAnarchyBotNotifier notifier,
        IGameArchiveService gameArchiveService,
        IGameResultDescriber gameResultDescriber,
        IUnitOfWork unitOfWork
    )
    {
        _gameToken = this.GetPrimaryKeyString();

        _logger = logger;
        _state = state;
        _core = core;
        _anarchyBotService = anarchyBotService;
        _notifier = notifier;
        _gameArchiveService = gameArchiveService;
        _gameResultDescriber = gameResultDescriber;
        _unitOfWork = unitOfWork;
    }

    public async Task StartGameAsync(GamePlayer player, CancellationToken token = default)
    {
        GameCoreState core = new();
        GamePlayer botPlayer = new(
            UserId: UserId.AnarchyBot(),
            Color: player.Color.Invert(),
            UserName: "Anarchy Bot",
            CountryCode: "XX",
            Rating: 161660
        );

        _state.State.CurrentGame = new AnarchyBotGameData()
        {
            Players = new(
                WhitePlayer: player.Color is GameColor.White ? player : botPlayer,
                BlackPlayer: player.Color is GameColor.Black ? player : botPlayer
            ),
            HumanColor = player.Color,
            BotColor = botPlayer.Color,
            InitialFen = _core.StartGame(core).FullFen,
            Core = core,
        };

        if (player.Color is GameColor.Black)
        {
            await PlayBotMoveAsync(_state.State.CurrentGame, token);
        }

        await _state.WriteStateAsync(token);
    }

    public Task<ErrorOr<AnarchyBotGameState>> GetStateAsync(CancellationToken token = default)
    {
        if (!TryGetCurrentGame(out var game))
        {
            return Task.FromResult<ErrorOr<AnarchyBotGameState>>(GameErrors.GameNotFound);
        }

        return Task.FromResult<ErrorOr<AnarchyBotGameState>>(GetGameState(game));
    }

    public async Task<ErrorOr<Success>> PlayMoveAsync(
        UserId userId,
        MoveKey moveKey,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
        {
            return GameErrors.GameNotFound;
        }

        GameColor sideToMove = _core.SideToMove(game.Core);
        if (!game.Players.TryGetPlayerById(userId, out var player) || player.Color != sideToMove)
        {
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(moveKey, game.Core);
        if (makeMoveResult.IsError)
        {
            return makeMoveResult.Errors;
        }
        var moveResult = makeMoveResult.Value;

        var nextSideToMove = _core.SideToMove(game.Core);
        var moveSnapshot = game.MoveHistory.AddMove(
            nextPlayer: nextSideToMove,
            moveResult,
            timeLeft: 0
        );
        await _notifier.NotifyPlayerMadeMoveAsync(
            _gameToken,
            moveSnapshot,
            plyNumber: game.MoveHistory.Moves.Count,
            didMoveEndGame: moveResult.EndStatus is not null
        );

        if (moveResult.EndStatus is not null)
        {
            await EndGameAsync(moveResult.EndStatus, game, token);
        }
        else if (nextSideToMove == game.BotColor)
        {
            await PlayBotMoveAsync(game, token);
        }

        await _state.WriteStateAsync(token);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ResignAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
        {
            return GameErrors.GameNotFound;
        }

        if (!game.Players.TryGetPlayerById(userId, out var player))
        {
            return GameErrors.PlayerInvalid;
        }

        await EndGameAsync(_gameResultDescriber.Resignation(by: player.Color), game, token);
        await _state.WriteStateAsync(token);
        return Result.Success;
    }

    private async Task PlayBotMoveAsync(AnarchyBotGameData game, CancellationToken token = default)
    {
        IReadOnlyChessBoard board = _core.GetReadOnlyBoard(game.Core);

        var botMoveResult = await _anarchyBotService.FindBestMoveAsync(board, token);
        if (botMoveResult.FirstError == AnarchyBotErrors.BotOffline)
        {
            await EndGameAsync(_gameResultDescriber.AnarchyBotOffline(game.BotColor), game, token);
            return;
        }
        else if (botMoveResult.IsError)
        {
            await EndGameAsync(_gameResultDescriber.AnarchyBotFailure(game.BotColor), game, token);
            return;
        }
        var botMove = botMoveResult.Value;

        LegalMoveSet legalMoves = _core.GetLegalMoves(game.Core);
        Move? legalMove = legalMoves.AllMoves.FirstOrDefault(move =>
        {
            List<AlgebraicPoint> captures = [.. move.Captures.Select(x => x.Position)];
            return move.From == botMove.From
                && move.To == botMove.To
                && move.PromotesTo == botMove.PromotesTo
                && captures.SequenceEqual(botMove.Captures ?? []);
        });
        if (legalMove is null)
        {
            _logger.LogError(
                "Anarchy Bot tried to play an illegal move ({BotMove}) on game {BotToken}",
                botMove,
                _gameToken
            );
            await EndGameAsync(
                _gameResultDescriber.AnarchyBotIllegalMove(game.BotColor),
                game,
                token
            );
            return;
        }

        var moveResult = _core.MakeMove(legalMove, game.Core);

        var moveSnapshot = game.MoveHistory.AddMove(
            nextPlayer: _core.SideToMove(game.Core),
            moveResult: moveResult,
            timeLeft: 0
        );
        var newLegalMoves = _core.EncodeLegalMoves(game.Core);
        await _notifier.NotifyBotMadeMoveAsync(
            _gameToken,
            moveSnapshot,
            plyNumber: game.MoveHistory.Moves.Count,
            compressedLegalMoves: newLegalMoves
        );

        if (moveResult.EndStatus is not null)
        {
            await EndGameAsync(moveResult.EndStatus, game, token);
        }
    }

    private async Task EndGameAsync(
        GameEndStatus endStatus,
        AnarchyBotGameData game,
        CancellationToken token = default
    )
    {
        if (game.Result is not null)
        {
            return;
        }

        GameResultData resultData = new(
            Result: endStatus.Result,
            ResultDescription: endStatus.ResultDescription,
            WhiteRatingChange: null,
            BlackRatingChange: null
        );

        await _notifier.NotifyGameEndedAsync(_gameToken, resultData);
        await _gameArchiveService.CreateArchiveAsync(
            _gameToken,
            pool: new PoolKey(
                PoolType.Casual,
                TimeControl: new(BaseSeconds: 0, IncrementSeconds: 0)
            ),
            whitePlayer: game.Players.WhitePlayer,
            blackPlayer: game.Players.BlackPlayer,
            endStatus: endStatus,
            token
        );
        await _unitOfWork.CompleteAsync(token);

        game.Result = resultData;
    }

    private AnarchyBotGameState GetGameState(AnarchyBotGameData game) =>
        new(
            WhitePlayer: game.Players.WhitePlayer,
            BlackPlayer: game.Players.BlackPlayer,
            SideToMove: _core.SideToMove(game.Core),
            InitialFen: game.InitialFen,
            MoveHistory: game.MoveHistory.Moves,
            LegalMoves: _core.GetLegalMoves(game.Core).MovePaths,
            ResultData: game.Result
        );

    private bool TryGetCurrentGame([NotNullWhen(true)] out AnarchyBotGameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null;
    }

    private bool TryGetOngoingGame([NotNullWhen(true)] out AnarchyBotGameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null && state.Result is null;
    }
}
