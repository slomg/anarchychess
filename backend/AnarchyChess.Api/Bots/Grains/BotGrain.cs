using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Errors;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Streaming;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;
using ErrorOr;
using Orleans.Streams;

namespace AnarchyChess.Api.Bots.Grains;

[Alias("AnarchyChess.Api.Bots.Grains.IBotGrain")]
public interface IBotGrain : IGrainWithStringKey
{
    [Alias("SyncPlyNumberAsync")]
    Task<ErrorOr<Success>> SyncPlyNumberAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    );

    [Alias("CreateAsync")]
    Task StartGameAsync(GamePlayer player, BotType botType, CancellationToken token = default);

    [Alias("GetStateAsync")]
    Task<ErrorOr<BotGameState>> GetStateAsync(CancellationToken token = default);

    [Alias("GetMovesAsync")]
    Task<ErrorOr<IReadOnlyList<Move>>> GetMovesAsync();

    [Alias("PlayMoveAsync")]
    Task<ErrorOr<Success>> PlayMoveAsync(
        UserId userId,
        MoveKey moveKey,
        CancellationToken token = default
    );

    [Alias("PlayBotMoveAsync")]
    Task PlayBotMoveAsync(ErrorOr<MoveEvaluation> botMoveResult, CancellationToken token = default);

    [Alias("ResignAsync")]
    Task<ErrorOr<Success>> ResignAsync(UserId userId, CancellationToken token = default);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Bots.Grains.BotGameData")]
public class BotGameData
{
    [Id(0)]
    public required PlayerRoster Players { get; init; }

    [Id(1)]
    public required GameColor HumanColor { get; init; }

    [Id(2)]
    public required GameColor BotColor { get; init; }

    [Id(7)]
    public required BotType BotType { get; init; }

    [Id(8)]
    public int LastEval { get; set; }

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
[Alias("AnarchyChess.Api.Bots.Grains.BotGrainState")]
public class BotGrainState
{
    [Id(0)]
    public BotGameData? CurrentGame { get; set; }
}

public class BotGrain : Grain, IBotGrain
{
    public const string StateName = "botGame";

    private readonly ILogger<BotGrain> _logger;
    private readonly IPersistentState<BotGrainState> _state;

    private readonly Dictionary<BotType, IBot> _bots;
    private readonly IGameCore _core;
    private readonly IBotMoveRunner _botMoveRunner;
    private readonly IBotNotifier _notifier;
    private readonly IGameArchiveService _gameArchiveService;
    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GameToken _gameToken;

    public BotGrain(
        ILogger<BotGrain> logger,
        [PersistentState(StateName)] IPersistentState<BotGrainState> state,
        IEnumerable<IBot> bots,
        IGameCore core,
        IBotMoveRunner botMoveRunner,
        IBotNotifier notifier,
        IGameArchiveService gameArchiveService,
        IGameResultDescriber gameResultDescriber,
        IUnitOfWork unitOfWork
    )
    {
        _gameToken = this.GetPrimaryKeyString();

        _logger = logger;
        _state = state;

        _bots = bots.ToDictionary(x => x.Type);
        _core = core;
        _botMoveRunner = botMoveRunner;
        _notifier = notifier;
        _gameArchiveService = gameArchiveService;
        _gameResultDescriber = gameResultDescriber;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> SyncPlyNumberAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        if (!TryGetCurrentGame(out var game))
        {
            return GameErrors.GameNotFound;
        }
        await _notifier.SyncPlyNumberAsync(game.MoveHistory.Moves.Count, connectionId);
        return Result.Success;
    }

    public async Task StartGameAsync(
        GamePlayer player,
        BotType botType,
        CancellationToken token = default
    )
    {
        IBot bot = _bots[botType];

        GameCoreState core = new();
        GamePlayer botPlayer = bot.CreateBotPlayer(color: player.Color.Invert());

        BotGameData game = new()
        {
            Players = new(
                WhitePlayer: player.Color is GameColor.White ? player : botPlayer,
                BlackPlayer: player.Color is GameColor.Black ? player : botPlayer
            ),
            HumanColor = player.Color,
            BotColor = botPlayer.Color,
            BotType = botType,
            InitialFen = _core.StartGame(core).FullFen,
            Core = core,
        };
        _state.State.CurrentGame = game;

        if (player.Color is GameColor.Black)
        {
            IReadOnlyChessBoard board = _core.GetReadOnlyBoard(game.Core);
            _botMoveRunner.RunMove(board, lastEval: 0, _gameToken, bot);
        }

        await _state.WriteStateAsync(token);
    }

    public Task<ErrorOr<BotGameState>> GetStateAsync(CancellationToken token = default)
    {
        if (!TryGetCurrentGame(out var game))
        {
            return Task.FromResult<ErrorOr<BotGameState>>(GameErrors.GameNotFound);
        }

        return Task.FromResult<ErrorOr<BotGameState>>(
            new BotGameState(
                WhitePlayer: game.Players.WhitePlayer,
                BlackPlayer: game.Players.BlackPlayer,
                BotColor: game.BotColor,
                BotType: game.BotType,
                SideToMove: _core.SideToMove(game.Core),
                InitialFen: game.InitialFen,
                MoveHistory: game.MoveHistory.Moves,
                LegalMoves: _core.GetLegalMoves(game.Core).MovePaths,
                ResultData: game.Result
            )
        );
    }

    public Task<ErrorOr<IReadOnlyList<Move>>> GetMovesAsync() =>
        Task.FromResult(
            TryGetCurrentGame(out var game)
                ? game.Core.Board.Moves.ToErrorOr()
                : GameErrors.GameNotFound
        );

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
            IReadOnlyChessBoard board = _core.GetReadOnlyBoard(game.Core);
            _botMoveRunner.RunMove(board, lastEval: game.LastEval, _gameToken, _bots[game.BotType]);
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

    public async Task PlayBotMoveAsync(
        ErrorOr<MoveEvaluation> botMoveResult,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
        {
            return;
        }

        if (botMoveResult.FirstError == BotErrors.BotOffline)
        {
            await EndGameAsync(_gameResultDescriber.BotOffline(game.BotColor), game, token);
            await _state.WriteStateAsync(token);
            return;
        }
        else if (botMoveResult.IsError)
        {
            await EndGameAsync(_gameResultDescriber.BotFailure(game.BotColor), game, token);
            await _state.WriteStateAsync(token);
            return;
        }
        var botMove = botMoveResult.Value;

        LegalMoveSet legalMoves = _core.GetLegalMoves(game.Core);
        Move? legalMove = legalMoves.FindBotMove(botMove.Move);
        if (legalMove is null)
        {
            _logger.LogError(
                "Anarchy Bot tried to play an illegal move ({BotMove}) on game {BotToken} (from {From} to {To})",
                botMove,
                _gameToken,
                AlgebraicPoint.FromIdx(botMove.Move.From),
                AlgebraicPoint.FromIdx(botMove.Move.To)
            );
            await EndGameAsync(_gameResultDescriber.BotIllegalMove(game.BotColor), game, token);
            return;
        }

        var moveResult = _core.MakeMove(legalMove, game.Core);
        if (moveResult.EndStatus is not null)
        {
            await EndGameAsync(moveResult.EndStatus, game, token);
        }

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
            compressedLegalMoves: newLegalMoves,
            evalForBot: botMove.EvalForBot,
            didMoveEndGame: moveResult.EndStatus is not null
        );

        game.LastEval = botMove.EvalForBot;

        await _state.WriteStateAsync(token);
    }

    private async Task EndGameAsync(
        GameEndStatus endStatus,
        BotGameData game,
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
        try
        {
            await _gameArchiveService.CreateBotArchiveAsync(
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save bot game");
        }

        var streamProvider = this.GetStreamProvider(StreamingConstants.StreamProvider);
        await streamProvider
            .GetStream<BotGameEndedEvent>(
                nameof(BotGameEndedEvent),
                game.Players.GetPlayerByColor(game.HumanColor).UserId
            )
            .OnNextAsync(new(GameToken: _gameToken, EndStatus: resultData));

        game.Result = resultData;
    }

    private bool TryGetCurrentGame([NotNullWhen(true)] out BotGameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null;
    }

    private bool TryGetOngoingGame([NotNullWhen(true)] out BotGameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null && state.Result is null;
    }
}
