using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Lobby.Errors;
using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Extensions;
using AnarchyChess.Api.Matchmaking.Grains;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Streaming;
using ErrorOr;
using Microsoft.Extensions.Options;
using Orleans.Streams;

namespace AnarchyChess.Api.Lobby.Grains;

[Alias("AnarchyChess.Api.Lobby.Grains.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey, ISeekObserver
{
    [Alias("CreateSeekAsync")]
    Task<ErrorOr<Created>> CreateSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        PoolKey pool,
        CancellationToken token = default
    );

    [Alias("CleanupConnectionAsync")]
    Task CleanupConnectionAsync(ConnectionId connectionId, CancellationToken token = default);

    [Alias("CancelSeekAsync")]
    Task CancelSeekAsync(PoolKey pool, CancellationToken token = default);

    [Alias("MatchWithOpenSeekAsync")]
    Task<ErrorOr<Created>> MatchWithOpenSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        UserId matchWith,
        PoolKey pool,
        CancellationToken token = default
    );

    [Alias("GetOngoingGames")]
    Task<List<OngoingGame>> GetOngoingGamesAsync();
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Lobby.Grains.PlayerSessionState")]
public class PlayerSessionState
{
    [Id(0)]
    public PlayerConnectionPoolMap ConnectionMap { get; } = new();

    [Id(1)]
    public Dictionary<GameToken, OngoingGame> OngoingGames { get; } = [];

    [Id(2)]
    public BoundedSet<GameToken> RecentlyRemoved { get; } = new(10);
}

// orleans testkit doesn't support 2 persistent states of the same type
[GenerateSerializer]
[Alias("AnarchyChess.Api.Lobby.Grains.PlayerSessionStartStreamState")]
public class PlayerSessionStartStreamState : StreamState;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Lobby.Grains.PlayerSessionEndStreamState")]
public class PlayerSessionEndStreamState : StreamState;

[ImplicitStreamSubscription(nameof(GameEndedEvent))]
[ImplicitStreamSubscription(nameof(GameStartedEvent))]
public class PlayerSessionGrain
    : Grain,
        IPlayerSessionGrain,
        IAsyncObserver<GameStartedEvent>,
        IAsyncObserver<GameEndedEvent>
{
    public const string StateName = "playerSession";

    private readonly UserId _userId;

    private readonly Dictionary<PoolKey, ConnectionId> _poolConnectionReservations = [];
    private readonly HashSet<ConnectionId> _connectionsRecentlyMatched = [];

    private readonly IPersistentState<PlayerSessionStartStreamState> _startStreamState;
    private readonly IPersistentState<PlayerSessionEndStreamState> _endStreamState;
    private readonly IPersistentState<PlayerSessionState> _state;
    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly ILobbyNotifier _lobbyNotifier;
    private readonly LobbySettings _settings;

    public PlayerSessionGrain(
        [PersistentState(StateName)] IPersistentState<PlayerSessionState> state,
        [PersistentState(StateName + "GameStartStream")]
            IPersistentState<PlayerSessionStartStreamState> startStreamState,
        [PersistentState(StateName + "GameEndStream")]
            IPersistentState<PlayerSessionEndStreamState> endStreamState,
        ILogger<PlayerSessionGrain> logger,
        ILobbyNotifier lobbyNotifier,
        IOptions<AppSettings> settings
    )
    {
        _userId = this.GetPrimaryKeyString();

        _state = state;
        _logger = logger;
        _lobbyNotifier = lobbyNotifier;
        _settings = settings.Value.Lobby;

        _startStreamState = startStreamState;
        _endStreamState = endStreamState;
    }

    public async Task<ErrorOr<Created>> CreateSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        PoolKey pool,
        CancellationToken token = default
    )
    {
        if (HasReachedGameLimit())
            return PlayerSessionErrors.TooManyGames;

        if (IsConnectionTaken(connectionId))
            return PlayerSessionErrors.ConnectionInGame;

        var matchmakingGrain = GrainFactory.GetMatchmakingGrain(pool);
        await matchmakingGrain.AddSeekAsync(seeker, this.AsSafeReference<ISeekObserver>(), token);

        _state.State.ConnectionMap.AddConnectionToPool(connectionId, pool);
        await _state.WriteStateAsync(token);

        return Result.Created;
    }

    public async Task CleanupConnectionAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        await RemoveConnectionFromPoolsAsync(connectionId, token);
        _connectionsRecentlyMatched.Remove(connectionId);
        await _state.WriteStateAsync(token);
    }

    public Task CancelSeekAsync(PoolKey pool, CancellationToken token = default) =>
        GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId, token);

    public async Task<ErrorOr<Created>> MatchWithOpenSeekAsync(
        ConnectionId connectionId,
        Seeker seeker,
        UserId matchWith,
        PoolKey pool,
        CancellationToken token = default
    )
    {
        if (HasReachedGameLimit())
            return PlayerSessionErrors.TooManyGames;

        if (IsConnectionTaken(connectionId))
            return PlayerSessionErrors.ConnectionInGame;

        _state.State.ConnectionMap.AddConnectionToPool(connectionId, pool);
        var startGameResult = await GrainFactory
            .GetMatchmakingGrain(pool)
            .MatchWithSeekerAsync(seeker, matchWith, token);
        if (startGameResult.IsError)
            return startGameResult.Errors;

        await _state.WriteStateAsync(token);
        return Result.Created;
    }

    public async Task SeekRemovedAsync(PoolKey pool, CancellationToken token = default)
    {
        _poolConnectionReservations.Remove(pool);

        var poolConnectionIds = _state.State.ConnectionMap.RemovePool(pool);
        await _state.WriteStateAsync(token);
        await _lobbyNotifier.NotifySeekFailedAsync(poolConnectionIds, pool);
    }

    public Task<bool> TryReserveSeekAsync(PoolKey pool)
    {
        if (HasReachedGameLimit())
            return Task.FromResult(false);
        if (_poolConnectionReservations.ContainsKey(pool))
            return Task.FromResult(false);

        var connectionIds = _state.State.ConnectionMap.PoolConnections(pool);

        foreach (var connectionId in connectionIds)
        {
            if (!IsConnectionTaken(connectionId))
            {
                _poolConnectionReservations[pool] = connectionId;
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task ReleaseReservationAsync(PoolKey pool)
    {
        _poolConnectionReservations.Remove(pool);
        return Task.CompletedTask;
    }

    public Task<List<OngoingGame>> GetOngoingGamesAsync() =>
        Task.FromResult<List<OngoingGame>>([.. _state.State.OngoingGames.Values]);

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        var streamProvider = this.GetStreamProvider(StreamingConstants.StreamProvider);

        var startedStream = streamProvider.GetStream<GameStartedEvent>(
            nameof(GameStartedEvent),
            this.GetPrimaryKeyString()
        );
        await startedStream.SubscribeAsync(OnNextAsync, _startStreamState.State.SequenceToken);

        var endedStream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );
        await endedStream.SubscribeAsync(OnNextAsync, _endStreamState.State.SequenceToken);

        await PruneOverGamesAsync();
    }

    public async Task OnNextAsync(GameStartedEvent @event, StreamSequenceToken? token = null)
    {
        if (!_startStreamState.State.TryUpdateSequenceToken(token))
            return;
        await _startStreamState.WriteStateAsync();

        var game = @event.Game;
        if (_state.State.RecentlyRemoved.Has(game.GameToken))
            return;

        _state.State.OngoingGames.TryAdd(game.GameToken, game);
        if (HasReachedGameLimit())
            await CancelAllSeeksAsync();

        if (@event.GameSource is GameSource.Matchmaking)
            await MatchmakingGameMatchedAsync(game);

        await _state.WriteStateAsync();
    }

    public async Task OnNextAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        if (!_endStreamState.State.TryUpdateSequenceToken(token))
            return;
        await _endStreamState.WriteStateAsync();

        _state.State.OngoingGames.Remove(@event.GameToken);
        _state.State.RecentlyRemoved.TryAdd(@event.GameToken);
        await _state.WriteStateAsync();

        await _lobbyNotifier.NotifyOngoingGameEndedAsync(_userId, @event.GameToken);
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error in player session grain game stream");
        return Task.CompletedTask;
    }

    private async Task MatchmakingGameMatchedAsync(OngoingGame game)
    {
        var connectionIds = _state.State.ConnectionMap.RemovePool(game.Pool);

        _poolConnectionReservations.Remove(game.Pool);
        _connectionsRecentlyMatched.UnionWith(connectionIds);

        if (connectionIds.Count > 0)
            await _lobbyNotifier.NotifyGameFoundAsync(_userId, connectionIds, game);

        foreach (var connectionId in connectionIds)
            await RemoveConnectionFromPoolsAsync(connectionId);
    }

    private async Task CancelAllSeeksAsync(CancellationToken token = default)
    {
        foreach (var pool in _state.State.ConnectionMap.ActivePools)
        {
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId, token);
        }
        _state.State.ConnectionMap.RemoveAllPools();
    }

    private async Task RemoveConnectionFromPoolsAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    )
    {
        var removedPools = _state.State.ConnectionMap.RemoveConnection(connectionId);
        foreach (var pool in removedPools)
        {
            await GrainFactory.GetMatchmakingGrain(pool).TryCancelSeekAsync(_userId, token);
        }
    }

    private bool IsConnectionTaken(ConnectionId connectionId) =>
        _connectionsRecentlyMatched.Contains(connectionId)
        || _poolConnectionReservations.Values.Any(claimedConn => connectionId == claimedConn);

    private bool HasReachedGameLimit() =>
        _state.State.OngoingGames.Count + _poolConnectionReservations.Count
        >= _settings.MaxActiveGames;

    private async Task PruneOverGamesAsync()
    {
        foreach (var gameToken in _state.State.OngoingGames.Keys.ToArray())
        {
            IGameGrain gameGrain = GrainFactory.GetGrain<IGameGrain>(gameToken);
            bool isGameOngoing = await gameGrain.IsGameOngoingAsync();
            if (!isGameOngoing)
            {
                _state.State.OngoingGames.Remove(gameToken);
            }
        }
    }
}
