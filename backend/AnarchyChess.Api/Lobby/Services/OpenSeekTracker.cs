using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;

namespace AnarchyChess.Api.Lobby.Services;

public class SeekWatcher
{
    public required HashSet<ConnectionId> ConnectionIds { get; init; }
    public required Seeker Seeker { get; init; }
}

public class OpenSeekEntry
{
    public required OpenSeek OpenSeek { get; init; }
    public required Seeker Seeker { get; init; }
    public required HashSet<string> SubscribedUserIds { get; init; }
}

public interface IOpenSeekTracker
{
    OpenSeekEntry AddSeek(Seeker seeker, PoolKey pool);
    OpenSeekEntry? RemoveSeek(UserId userId, PoolKey pool);
    List<OpenSeek> Subscribe(ConnectionId connectionId, Seeker watchingSeeker);
    void Unsubscribe(UserId userId, ConnectionId connectionId);
    void Clear();
}

public class OpenSeekTracker(IRandomProvider randomProvider) : IOpenSeekTracker
{
    public const int MAX_INITIAL_SEEKS = 10;

    private readonly Dictionary<UserId, Dictionary<PoolKey, OpenSeekEntry>> _userOpenSeeks = [];
    private readonly Dictionary<UserId, SeekWatcher> _connections = [];

    private readonly IRandomProvider _randomProvider = randomProvider;

    public List<OpenSeek> Subscribe(ConnectionId connectionId, Seeker watchingSeeker)
    {
        if (_connections.TryGetValue(watchingSeeker.UserId, out var existingConnection))
        {
            existingConnection.ConnectionIds.Add(connectionId);
        }
        else
        {
            _connections[watchingSeeker.UserId] = new()
            {
                ConnectionIds = [connectionId],
                Seeker = watchingSeeker,
            };
        }

        var compatibleUsers = GetRandomCompatibleUsers(watchingSeeker);

        List<OpenSeek> subscribedTo = new(MAX_INITIAL_SEEKS);
        while (subscribedTo.Count < MAX_INITIAL_SEEKS)
        {
            bool addedThisRound = false;

            foreach (var (userId, seekEntries) in compatibleUsers)
            {
                if (seekEntries.TryDequeue(out var entry))
                {
                    subscribedTo.Add(entry.OpenSeek);
                    addedThisRound = true;
                }

                if (subscribedTo.Count >= MAX_INITIAL_SEEKS)
                {
                    break;
                }
            }

            if (!addedThisRound)
            {
                break;
            }
        }

        return subscribedTo;
    }

    public void Unsubscribe(UserId userId, ConnectionId connectionId)
    {
        if (!_connections.TryGetValue(userId, out var existingConnection))
            return;

        existingConnection.ConnectionIds.Remove(connectionId);
        if (existingConnection.ConnectionIds.Count == 0)
        {
            _connections.Remove(userId);
        }
    }

    public IReadOnlyCollection<ConnectionId>? GetUserConnectionIds(UserId userId) =>
        _connections.GetValueOrDefault(userId)?.ConnectionIds;

    public OpenSeekEntry AddSeek(Seeker seeker, PoolKey pool)
    {
        int? rating = seeker is RatedSeeker ratedSeeker ? ratedSeeker.Rating.Value : null;

        HashSet<string> matchingUserIds = [];
        foreach (var (userId, watcher) in _connections)
        {
            if (watcher.Seeker.IsCompatibleWith(seeker) && seeker.IsCompatibleWith(watcher.Seeker))
            {
                matchingUserIds.Add(userId);
            }
        }

        OpenSeek openSeek = new(UserId: seeker.UserId, seeker.UserName, pool, rating);
        OpenSeekEntry entry = new()
        {
            OpenSeek = openSeek,
            Seeker = seeker,
            SubscribedUserIds = matchingUserIds,
        };

        if (_userOpenSeeks.TryGetValue(seeker.UserId, out var byPool))
        {
            byPool[pool] = entry;
        }
        else
        {
            _userOpenSeeks[seeker.UserId] = new() { [pool] = entry };
        }

        return entry;
    }

    public OpenSeekEntry? RemoveSeek(UserId userId, PoolKey pool)
    {
        if (!_userOpenSeeks.TryGetValue(userId, out var byPool))
        {
            return null;
        }

        if (!byPool.TryGetValue(pool, out var entry))
        {
            return null;
        }

        byPool.Remove(pool);
        if (byPool.Count == 0)
        {
            _userOpenSeeks.Remove(userId);
        }

        return entry;
    }

    public void Clear()
    {
        _connections.Clear();
        _userOpenSeeks.Clear();
    }

    private Dictionary<UserId, Queue<OpenSeekEntry>> GetRandomCompatibleUsers(Seeker watchingSeeker)
    {
        var users = _userOpenSeeks.Keys.ToList();

        Dictionary<UserId, Queue<OpenSeekEntry>> result = [];
        while (result.Count < MAX_INITIAL_SEEKS && users.Count > 0)
        {
            var candidateIdx = _randomProvider.Next(users.Count);
            var candidateUserId = users[candidateIdx];

            Queue<OpenSeekEntry> compatibleEntries = [];
            foreach (var entry in _userOpenSeeks[candidateUserId].Values)
            {
                if (
                    watchingSeeker.IsCompatibleWith(entry.Seeker)
                    && entry.Seeker.IsCompatibleWith(watchingSeeker)
                )
                {
                    compatibleEntries.Enqueue(entry);
                }
            }

            users[candidateIdx] = users[^1];
            users.RemoveAt(users.Count - 1);
            if (compatibleEntries.Count == 0)
                continue;

            result[candidateUserId] = compatibleEntries;
        }

        return result;
    }
}
