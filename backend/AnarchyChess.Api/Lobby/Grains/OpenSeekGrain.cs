using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using Orleans.Streams;

namespace AnarchyChess.Api.Lobby.Grains;

[Alias("AnarchyChess.Api.Lobby.Grains.IOpenSeekGrain")]
public interface IOpenSeekGrain : IGrainWithIntegerKey
{
    [Alias("Subscribe")]
    Task SubscribeAsync(ConnectionId connectionId, Seeker seeker);

    [Alias("Unsubscribe")]
    Task UnsubscribeAsync(UserId userId, ConnectionId connectionId);

    [Alias("InitializeAsync")]
    Task InitializeAsync();

#if DEBUG
    [Alias("ClearStateAsync")]
    Task ClearStateAsync();
#endif
}

[KeepAlive]
public class OpenSeekGrain(
    ILogger<OpenSeekGrain> logger,
    IOpenSeekNotifier openSeekNotifier,
    IOpenSeekTracker openSeekTracker
) : Grain, IOpenSeekGrain
{
    public const int RefetchTimer = 0;
    public const string StateName = "openSeek";

    private readonly ILogger<OpenSeekGrain> _logger = logger;
    private readonly IOpenSeekNotifier _openSeekNotifier = openSeekNotifier;
    private readonly IOpenSeekTracker _openSeekTracker = openSeekTracker;

    public async Task SubscribeAsync(ConnectionId connectionId, Seeker seeker)
    {
        var subscribedTo = _openSeekTracker.Subscribe(connectionId, seeker);
        if (subscribedTo.Count > 0)
        {
            await _openSeekNotifier.NotifyOpenSeekAsync(connectionId, subscribedTo);
        }
    }

    public Task UnsubscribeAsync(UserId userId, ConnectionId connectionId)
    {
        _openSeekTracker.Unsubscribe(userId, connectionId);
        return Task.CompletedTask;
    }

    private async Task OnSeekCreated(OpenSeekCreatedEvent @event, StreamSequenceToken _)
    {
        var addedEntry = _openSeekTracker.AddSeek(@event.Seeker, @event.Pool);
        if (addedEntry.SubscribedUserIds.Count > 0)
        {
            await _openSeekNotifier.NotifyOpenSeekAsync(
                addedEntry.SubscribedUserIds,
                [addedEntry.OpenSeek]
            );
        }
    }

    private async Task OnSeekEnded(OpenSeekRemovedEvent @event, StreamSequenceToken _)
    {
        var removedEntry = _openSeekTracker.RemoveSeek(@event.UserId, @event.Pool);
        if (removedEntry?.SubscribedUserIds.Count > 0)
        {
            await _openSeekNotifier.NotifyOpenSeekEndedAsync(
                removedEntry.SubscribedUserIds,
                @event.UserId,
                @event.Pool
            );
        }
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);

        var createdStream = streamProvider.GetStream<OpenSeekCreatedEvent>(
            nameof(OpenSeekCreatedEvent)
        );
        await createdStream.SubscribeOrResumeAsync(OnSeekCreated);

        var removedStream = streamProvider.GetStream<OpenSeekRemovedEvent>(
            nameof(OpenSeekRemovedEvent)
        );
        await removedStream.SubscribeOrResumeAsync(OnSeekEnded);

        await base.OnActivateAsync(cancellationToken);
    }

    public Task InitializeAsync() => Task.CompletedTask;

#if DEBUG
    public Task ClearStateAsync()
    {
        _openSeekTracker.Clear();
        return Task.CompletedTask;
    }
#endif
}
