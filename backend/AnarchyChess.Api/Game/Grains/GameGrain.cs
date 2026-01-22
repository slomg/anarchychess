using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Streaming;
using ErrorOr;
using Orleans.Streams;

namespace AnarchyChess.Api.Game.Grains;

public class GameGrain : Grain, IGameGrain, IRemindable
{
    public const string ClockReactivationReminder = "clockReactivationReminder";
    public const string StateName = "game";

    private readonly string _gameToken;

    private readonly ILogger<GameGrain> _logger;
    private readonly IPersistentState<GameGrainState> _state;
    private readonly IMoveHandler _moveHandler;
    private readonly IDrawHandler _drawHandler;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IGameFinalizer _gameFinalizer;
    private readonly IGameClock _clock;

    private IGrainTimer? _clockTimer;

    public GameGrain(
        ILogger<GameGrain> logger,
        [PersistentState(StateName)] IPersistentState<GameGrainState> state,
        IMoveHandler moveHandler,
        IDrawHandler drawHandler,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier,
        IGameFinalizer gameFinalizer
    )
    {
        _gameToken = this.GetPrimaryKeyString();

        _logger = logger;
        _state = state;
        _moveHandler = moveHandler;
        _drawHandler = drawHandler;
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
                        _gameToken,
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
                        _gameToken,
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
            _gameToken,
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

        var result = await _drawHandler.HandleDrawRequestAsync(player, _gameToken, game);
        if (result.IsError)
            return result.Errors;

        var endStatus = result.Value;
        if (endStatus is not null)
        {
            await EndGameAsync(endStatus, game, token);
        }
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

        var result = await _drawHandler.HandleDeclineDrawAsync(player, _gameToken, game);
        if (result.IsError)
            return result.Errors;

        await _state.WriteStateAsync(token);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> MovePieceAsync(
        UserId moveMadeBy,
        MoveKey key,
        CancellationToken token = default
    )
    {
        if (!TryGetOngoingGame(out var game))
            return GameErrors.GameNotFound;

        var result = await _moveHandler.HandleMoveAsync(moveMadeBy, key, _gameToken, game, token);
        if (result.IsError)
            return result.Errors;

        var endStatus = result.Value;
        if (endStatus is not null)
        {
            await EndGameAsync(endStatus, game, token);
        }
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

    private async Task OnClockTimerElapsedAsync(CancellationToken token = default)
    {
        if (!TryGetOngoingGame(out var game))
            return;

        var timeoutResult = _clock.DetectTimeout(
            tickingPlayer: _core.SideToMove(game.Core),
            game.ClockState
        );
        if (timeoutResult is null)
        {
            ScheduleTimeoutTimer(game);
            return;
        }

        await EndGameAsync(timeoutResult, game, token);
        await _state.WriteStateAsync(token);
    }

    private async Task EndGameAsync(
        GameEndStatus endStatus,
        GameData game,
        CancellationToken token = default
    )
    {
        if (game.Result is not null)
            return;

        _logger.LogInformation("Game {GameToken} eneded by {EndStatus}", _gameToken, endStatus);

        _clock.CommitLastTurn(_core.SideToMove(game.Core), game.ClockState);
        var state = GetGameState(game);

        game.Result = await _gameFinalizer.FinalizeGameAsync(_gameToken, state, endStatus, token);
        await _gameNotifier.NotifyGameEndedAsync(
            _gameToken,
            game.Result,
            _clock.ToSnapshot(game.ClockState),
            game.NotifierState
        );

        var streamProvider = this.GetStreamProvider(StreamingConstants.StreamProvider);
        GameEndedEvent endedEvent = new(_gameToken, game.Result);

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
        GameState gameState = new(
            Revision: game.NotifierState.Revision,
            GameSource: game.GameSource,
            Pool: game.Pool,
            WhitePlayer: game.Players.WhitePlayer,
            BlackPlayer: game.Players.BlackPlayer,
            Clocks: _clock.ToSnapshot(game.ClockState),
            SideToMove: _core.SideToMove(game.Core),
            InitialFen: game.InitialFen,
            LegalMoves: _core.GetLegalMoves(game.Core).MovePaths,
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
