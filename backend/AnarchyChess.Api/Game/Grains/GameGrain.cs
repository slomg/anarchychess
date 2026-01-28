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
    private readonly IClockHandler _clockHandler;
    private readonly IGameEndHandler _gameEndHandler;
    private readonly IOvertime _overtime;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly IGameNotifier _gameNotifier;
    private readonly IGameClock _clock;

    private IGrainTimer? _clockTimer;

    public GameGrain(
        ILogger<GameGrain> logger,
        [PersistentState(StateName)] IPersistentState<GameGrainState> state,
        IMoveHandler moveHandler,
        IDrawHandler drawHandler,
        IClockHandler clockHandler,
        IGameEndHandler gameEndHandler,
        IOvertime overtime,
        IGameCore core,
        IGameClock clock,
        IGameResultDescriber resultDescriber,
        IGameNotifier gameNotifier
    )
    {
        _gameToken = this.GetPrimaryKeyString();

        _logger = logger;
        _state = state;
        _moveHandler = moveHandler;
        _drawHandler = drawHandler;
        _clockHandler = clockHandler;
        _gameEndHandler = gameEndHandler;
        _overtime = overtime;
        _core = core;
        _clock = clock;
        _resultDescriber = resultDescriber;
        _gameNotifier = gameNotifier;
    }

    public async Task StartGameAsync(
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        PoolKey pool,
        GameSource gameSource,
        CancellationToken token = default
    )
    {
        GameCoreState core = new();
        _state.State.CurrentGame = new()
        {
            Players = new(whitePlayer, blackPlayer),
            GameSource = gameSource,
            Pool = pool,
            InitialFen = _core.StartGame(core).FullFen,
            Core = core,
            ClockState = _clock.Create(pool.TimeControl),
        };

        ScheduleTimeoutTimer(_clockHandler.GetClockDueTime(_state.State.CurrentGame));
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
            game.MoveHistory.Moves.Count < 2
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
        ScheduleTimeoutTimer(_clockHandler.GetClockDueTime(game));
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
            ScheduleTimeoutTimer(_clockHandler.GetClockDueTime(game));
        }

        return base.OnActivateAsync(cancellationToken);
    }

    private async Task OnClockTimerElapsedAsync(CancellationToken token = default)
    {
        if (!TryGetOngoingGame(out var game))
            return;

        var (rescheduleTo, endResult) = await _clockHandler.OnClockTickAsync(_gameToken, game);
        if (endResult is not null)
        {
            await EndGameAsync(endResult, game, token);
            await _state.WriteStateAsync(token);
        }
        if (rescheduleTo is not null)
        {
            ScheduleTimeoutTimer(rescheduleTo.Value);
        }
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

        var state = GetGameState(game);
        game.Result = await _gameEndHandler.HandleGameEndAsync(
            state,
            endStatus,
            _gameToken,
            game,
            token
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
            MoveHistory: game.MoveHistory.Moves,
            DrawState: game.DrawRequest.GetState(),
            ResultData: game.Result
        );
        return gameState;
    }

    private void ScheduleTimeoutTimer(TimeSpan dueTime)
    {
        _clockTimer?.Dispose();
        _clockTimer = this.RegisterGrainTimer(
            callback: OnClockTimerElapsedAsync,
            dueTime: dueTime,
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
