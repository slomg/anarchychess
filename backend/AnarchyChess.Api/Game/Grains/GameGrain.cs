using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Streaming;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;
using System.Diagnostics.CodeAnalysis;

namespace AnarchyChess.Api.Game.Grains;

public class GameGrain : Grain, IGameGrain, IRemindable
{
    public const string ClockReactivationReminder = "clockReactivationReminder";
    public const string StateName = "game";

    private readonly string _token;

    private readonly ILogger<GameGrain> _logger;
    private readonly IPersistentState<GameGrainState> _state;

    private readonly GameSettings _settings;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IGameFinalizer _gameFinalizer;
    private readonly IGameClock _clock;

    private IGrainTimer? _clockTimer;

    public GameGrain(
        ILogger<GameGrain> logger,
        [PersistentState(StateName)] IPersistentState<GameGrainState> state,
        IOptions<AppSettings> settings,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier,
        IGameFinalizer gameFinalizer
    )
    {
        _token = this.GetPrimaryKeyString();

        _logger = logger;
        _state = state;
        _settings = settings.Value.Game;
        _core = core;
        _clock = clock;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
        _gameFinalizer = gameFinalizer;
    }

    public async Task StartGameAsync(
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        PoolKey pool,
        GameSource gameSource,
        CancellationToken token = default
    )
    {
        PlayerRoster players = new(whitePlayer, blackPlayer);
        GameCoreState core = new();
        DrawRequestState drawRequest = new();
        GameNotifierState notifierState = new();
        GameClockState clockState = _clock.Create(pool.TimeControl);

        _state.State.CurrentGame = new()
        {
            Players = players,
            GameSource = gameSource,
            Pool = pool,
            InitialFen = _core.StartGame(core).FullFen,
            Core = core,
            DrawRequest = drawRequest,
            ClockState = clockState,
            NotifierState = notifierState,
        };

        ScheduleTimeoutTimer(_state.State.CurrentGame);
        await this.RegisterOrUpdateReminder(
            ClockReactivationReminder,
            dueTime: TimeSpan.FromMinutes(5),
            period: TimeSpan.FromMinutes(5)
        );

        var streamProvider = this.GetStreamProvider(StreamingConstants.StreamProvider);

        await streamProvider
            .GetStream<GameStartedEvent>(nameof(GameStartedEvent), whitePlayer.UserId)
            .OnNextAsync(
                new GameStartedEvent(
                    new OngoingGame(
                        _token,
                        pool,
                        Opponent: new(UserId: blackPlayer.UserId, UserName: blackPlayer.UserName)
                    ),
                    gameSource
                )
            );

        await streamProvider
            .GetStream<GameStartedEvent>(nameof(GameStartedEvent), blackPlayer.UserId)
            .OnNextAsync(
                new GameStartedEvent(
                    new OngoingGame(
                        _token,
                        pool,
                        Opponent: new(UserId: whitePlayer.UserId, UserName: whitePlayer.UserName)
                    ),
                    gameSource
                )
            );

        await _state.WriteStateAsync(token);
    }

    public async Task<ErrorOr<Success>> SyncRevisionAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        if (!TryGetCurrentGame(out var game))
            return GameErrors.GameNotFound;
        await _gameNotifier.SyncRevisionAsync(connectionId, game.NotifierState);
        return Result.Success;
    }

    public Task<ErrorOr<GameState>> GetStateAsync()
    {
        if (!TryGetCurrentGame(out var game))
            return Task.FromResult<ErrorOr<GameState>>(GameErrors.GameNotFound);

        var gameState = GetGameState(game);
        return Task.FromResult<ErrorOr<GameState>>(gameState);
    }

    public Task<ErrorOr<PlayerRoster>> GetPlayersAsync() =>
        Task.FromResult(
            TryGetCurrentGame(out var game) ? game.Players.ToErrorOr() : GameErrors.GameNotFound
        );

    public Task<ErrorOr<IReadOnlyList<Move>>> GetMovesAsync() =>
        Task.FromResult(
            TryGetCurrentGame(out var game)
                ? game.Core.Board.Moves.ToErrorOr()
                : GameErrors.GameNotFound
        );

    public async Task<ErrorOr<Success>> RequestGameEndAsync(
        UserId byUserId,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
            return GameErrors.GameNotFound;
        if (!game.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        GameEndStatus endStatus =
            game.MoveSnapshots.Count < 2
                ? _resultDescriber.Aborted(player.Color)
                : _resultDescriber.Resignation(player.Color);

        _logger.LogInformation(
            "Game {GameToken} ended by user {UserId}. Result: {Result}",
            _token,
            byUserId,
            endStatus.Result
        );
        await EndGameAsync(endStatus, game, token);
        await _state.WriteStateAsync(token);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RequestDrawAsync(
        UserId byUserId,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
            return GameErrors.GameNotFound;
        if (!game.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (game.DrawRequest.HasPendingRequest(player.Color))
        {
            await EndGameAsync(_resultDescriber.DrawByAgreement(), game, token);
            await _state.WriteStateAsync(token);
            return Result.Success;
        }

        var requestResult = game.DrawRequest.RequestDraw(player.Color);
        if (requestResult.IsError)
            return requestResult.Errors;

        await _gameNotifier.NotifyDrawStateChangeAsync(
            _token,
            game.DrawRequest.GetState(),
            game.NotifierState
        );
        await _state.WriteStateAsync(token);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeclineDrawAsync(
        UserId byUserId,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
            return GameErrors.GameNotFound;
        if (!game.Players.TryGetPlayerById(byUserId, out var player))
            return GameErrors.PlayerInvalid;

        if (!game.DrawRequest.TryDeclineDraw(player.Color, _settings.DrawCooldown))
            return GameErrors.DrawNotRequested;

        await _gameNotifier.NotifyDrawStateChangeAsync(
            _token,
            game.DrawRequest.GetState(),
            game.NotifierState
        );
        await _state.WriteStateAsync(token);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> MovePieceAsync(
        UserId byUserId,
        MoveKey key,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
            return GameErrors.GameNotFound;

        var currentPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        if (currentPlayer.UserId != byUserId)
        {
            _logger.LogWarning(
                "User {UserId} attmpted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                byUserId,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(key, game.Core);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;
        var moveResult = makeMoveResult.Value;

        var legalMoves = _core.GetLegalMoves(game.Core);
        var nextPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        var moveSnapshot = BuildAndStoreMove(
            movedBy: currentPlayer.Color,
            nextPlayer: nextPlayer.Color,
            moveResult,
            game
        );

        if (moveResult.EndStatus is not null)
        {
            await EndGameAsync(moveResult.EndStatus, game, token);
        }

        await _gameNotifier.NotifyMoveMadeAsync(
            notification: new(
                GameToken: _token,
                Move: moveSnapshot,
                PlyNumber: game.MoveSnapshots.Count,
                Clocks: _clock.ToSnapshot(game.ClockState),
                SideToMoveUserId: nextPlayer.UserId,
                EncodedLegalMoves: legalMoves.EncodedMoves,
                HasForcedMoves: legalMoves.HasForcedMoves
            ),
            game.NotifierState
        );
        await HandleDrawForMoveAsync(moveBy: currentPlayer.Color, game);
        await HandleClockForMoveAsync(game, token);
        await _state.WriteStateAsync(token);

        return Result.Success;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != ClockReactivationReminder)
            return;

        if (TryGetCurrentGame(out var game) && game.Result is not null)
        {
            var reminder = await this.GetReminder(ClockReactivationReminder);
            if (reminder is not null)
                await this.UnregisterReminder(reminder);
        }
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (TryGetOngoingGame(out var game))
        {
            ScheduleTimeoutTimer(game);
        }

        return base.OnActivateAsync(cancellationToken);
    }

    private MoveSnapshot BuildAndStoreMove(
        GameColor movedBy,
        GameColor nextPlayer,
        MoveResult moveResult,
        GameData game
    )
    {
        var timeLeft = _clock.CommitTurn(movedBy, game.ClockState);

        MoveSnapshot moveSnapshot = new(
            Path: moveResult.MovePath,
            Fen: moveResult.Fen.FullFen,
            NextSideToMove: nextPlayer,
            San: moveResult.San,
            timeLeft
        );
        game.MoveSnapshots.Add(moveSnapshot);
        return moveSnapshot;
    }

    private async Task HandleDrawForMoveAsync(GameColor moveBy, GameData game)
    {
        game.DrawRequest.DecrementCooldown();
        // auto decline the draw if it exists
        if (game.DrawRequest.TryDeclineDraw(moveBy, _settings.DrawCooldown))
        {
            await _gameNotifier.NotifyDrawStateChangeAsync(
                _token,
                game.DrawRequest.GetState(),
                game.NotifierState
            );
        }
    }

    private async Task HandleClockForMoveAsync(GameData game, CancellationToken token = default)
    {
        var didTimeOut = await EndGameIfTimedOutAsync(game, token);
        if (!didTimeOut)
        {
            ScheduleTimeoutTimer(game);
        }
    }

    private async Task OnClockTimerElapsedAsync(CancellationToken token = default)
    {
        if (!TryGetOngoingGame(out var game))
            return;

        var didTimeOut = await EndGameIfTimedOutAsync(game, token);
        if (!didTimeOut)
        {
            ScheduleTimeoutTimer(game);
            return;
        }
        await _state.WriteStateAsync(token);
    }

    private async Task<bool> EndGameIfTimedOutAsync(
        GameData game,
        CancellationToken token = default
    )
    {
        var timeoutResult = _clock.DetectTimeout(
            tickingPlayer: _core.SideToMove(game.Core),
            game.ClockState
        );
        if (timeoutResult is null)
        {
            return false;
        }

        await EndGameAsync(timeoutResult, game, token);
        return true;
    }

    private async Task EndGameAsync(
        GameEndStatus endStatus,
        GameData game,
        CancellationToken token = default
    )
    {
        if (game.Result is not null)
            return;

        _logger.LogInformation("Game {GameToken} eneded by {EndStatus}", _token, endStatus);

        _clock.CommitLastTurn(_core.SideToMove(game.Core), game.ClockState);
        var state = GetGameState(game);

        game.Result = await _gameFinalizer.FinalizeGameAsync(_token, state, endStatus, token);
        await _gameNotifier.NotifyGameEndedAsync(
            _token,
            game.Result,
            _clock.ToSnapshot(game.ClockState),
            game.NotifierState
        );

        var streamProvider = this.GetStreamProvider(StreamingConstants.StreamProvider);
        GameEndedEvent endedEvent = new(_token, game.Result);

        await streamProvider
            .GetStream<GameEndedEvent>(nameof(GameEndedEvent), game.Players.WhitePlayer.UserId)
            .OnNextAsync(endedEvent);
        await streamProvider
            .GetStream<GameEndedEvent>(nameof(GameEndedEvent), game.Players.BlackPlayer.UserId)
            .OnNextAsync(endedEvent);

        var reminder = await this.GetReminder(ClockReactivationReminder);
        if (reminder is not null)
            await this.UnregisterReminder(reminder);

        _clockTimer?.Dispose();
        _clockTimer = null;
    }

    private GameState GetGameState(GameData game)
    {
        var legalMoves = _core.GetLegalMoves(game.Core);
        MoveOptions moveOptions = new(
            LegalMoves: legalMoves.MovePaths,
            HasForcedMoves: legalMoves.HasForcedMoves
        );

        GameState gameState = new(
            Revision: game.NotifierState.Revision,
            GameSource: game.GameSource,
            Pool: game.Pool,
            WhitePlayer: game.Players.WhitePlayer,
            BlackPlayer: game.Players.BlackPlayer,
            Clocks: _clock.ToSnapshot(game.ClockState),
            SideToMove: _core.SideToMove(game.Core),
            InitialFen: game.InitialFen,
            MoveOptions: moveOptions,
            MoveHistory: game.MoveSnapshots,
            DrawState: game.DrawRequest.GetState(),
            ResultData: game.Result
        );
        return gameState;
    }

    private void ScheduleTimeoutTimer(GameData game)
    {
        _clockTimer?.Dispose();

        var sideToMove = _core.SideToMove(game.Core);
        _clockTimer = this.RegisterGrainTimer(
            callback: OnClockTimerElapsedAsync,
            dueTime: TimeSpan.FromMilliseconds(
                _clock.CalculateTimeLeftMs(sideToMove, game.ClockState)
            ),
            period: Timeout.InfiniteTimeSpan
        );
    }

    private bool TryGetCurrentGame([NotNullWhen(true)] out GameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null;
    }

    private bool TryGetOngoingGame([NotNullWhen(true)] out GameData? state)
    {
        state = _state.State.CurrentGame;
        return state is not null && state.Result is null;
    }
}
