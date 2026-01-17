using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Streaming;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using NSubstitute;
using Orleans.Providers.Streams.Common;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace AnarchyChess.Api.Unit.Tests.LobbyTests;

public class OpenSeekGrainTests : BaseGrainTest
{
    private readonly IOpenSeekNotifier _notifierMock = Substitute.For<IOpenSeekNotifier>();
    private readonly IOpenSeekTracker _trackerMock = Substitute.For<IOpenSeekTracker>();

    public OpenSeekGrainTests()
    {
        Silo.ServiceProvider.AddService(_notifierMock);
        Silo.ServiceProvider.AddService(_trackerMock);
    }

    private TestStream<OpenSeekCreatedEvent> ProbeOpenSeekCreatedStream() =>
        Silo.AddStreamProbe<OpenSeekCreatedEvent>(
            nameof(OpenSeekCreatedEvent),
            streamNamespace: null,
            StreamingConstants.StreamProvider
        );

    private TestStream<OpenSeekRemovedEvent> ProbeOpenSeekRemovedStream() =>
        Silo.AddStreamProbe<OpenSeekRemovedEvent>(
            nameof(OpenSeekRemovedEvent),
            streamNamespace: null,
            StreamingConstants.StreamProvider
        );

    [Fact]
    public async Task SubscribeAsync_notifies_of_all_compatible_seeks()
    {
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        ConnectionId connId = "conn 1";
        var watcher = new CasualSeekerFaker().Generate();
        var openSeeks = new OpenSeekFaker().Generate(5);
        _trackerMock.Subscribe(connId, watcher).Returns(openSeeks);

        await grain.SubscribeAsync(connId, watcher);

        await _notifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is(connId),
                Arg.Is<IEnumerable<OpenSeek>>(x => x.SequenceEqual(openSeeks))
            );
    }

    [Fact]
    public async Task SubscribeAsync_doesnt_notify_if_no_seeks_are_found()
    {
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        ConnectionId connId = "conn 1";
        var watcher = new CasualSeekerFaker().Generate();
        _trackerMock.Subscribe(connId, watcher).Returns([]);

        await grain.SubscribeAsync(connId, watcher);

        await _notifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyOpenSeekAsync(Arg.Any<ConnectionId>(), default!);
    }

    [Fact]
    public async Task UnsubscribeAsync_unsubscribes_user()
    {
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);
        UserId userId = "user 1";
        ConnectionId connId = "conn 1";

        await grain.UnsubscribeAsync(userId, connId);

        _trackerMock.Received(1).Unsubscribe(userId, connId);
    }

    [Fact]
    public async Task SeekCreatedEvent_notifies_all_subscribed()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var poolKey = new PoolKeyFaker().Generate();
        OpenSeekEntry entry = new()
        {
            OpenSeek = new OpenSeekFaker().Generate(),
            Seeker = new CasualSeekerFaker().Generate(),
            SubscribedUserIds = ["user 1", "user 2", "user 3"],
        };
        await Silo.CreateGrainAsync<OpenSeekGrain>(0);
        _trackerMock.AddSeek(entry.Seeker, poolKey).Returns(entry);

        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(entry.Seeker, poolKey),
            new EventSequenceToken()
        );

        List<OpenSeek> expectedSeeks = [entry.OpenSeek];
        await _notifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(entry.SubscribedUserIds)),
                Arg.Is<IEnumerable<OpenSeek>>(seeks => seeks.SequenceEqual(expectedSeeks))
            );
    }

    [Fact]
    public async Task SeekCreatedEvent_doesnt_notify_if_no_subscribers()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var poolKey = new PoolKeyFaker().Generate();
        OpenSeekEntry entry = new()
        {
            OpenSeek = new OpenSeekFaker().Generate(),
            Seeker = new CasualSeekerFaker().Generate(),
            SubscribedUserIds = [],
        };
        await Silo.CreateGrainAsync<OpenSeekGrain>(0);
        _trackerMock.AddSeek(entry.Seeker, poolKey).Returns(entry);

        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(entry.Seeker, poolKey),
            new EventSequenceToken()
        );

        await _notifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyOpenSeekAsync(Arg.Any<IEnumerable<string>>(), default!);
    }

    [Fact]
    public async Task SeekEndedEvent_notifies_all_subscribers()
    {
        var removeStream = ProbeOpenSeekRemovedStream();
        var poolKey = new PoolKeyFaker().Generate();
        OpenSeekEntry entry = new()
        {
            OpenSeek = new OpenSeekFaker().Generate(),
            Seeker = new CasualSeekerFaker().Generate(),
            SubscribedUserIds = ["user 1", "user 2", "user 3"],
        };
        await Silo.CreateGrainAsync<OpenSeekGrain>(0);
        _trackerMock.RemoveSeek(entry.Seeker.UserId, poolKey).Returns(entry);

        await removeStream.OnNextAsync(
            new OpenSeekRemovedEvent(entry.Seeker.UserId, poolKey),
            new EventSequenceToken()
        );

        await _notifierMock
            .Received(1)
            .NotifyOpenSeekEndedAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(entry.SubscribedUserIds)),
                entry.Seeker.UserId,
                poolKey
            );
    }

    [Fact]
    public async Task SeekEndedEvent_doesnt_notify_if_no_subscribers()
    {
        var removeStream = ProbeOpenSeekRemovedStream();
        var poolKey = new PoolKeyFaker().Generate();
        OpenSeekEntry entry = new()
        {
            OpenSeek = new OpenSeekFaker().Generate(),
            Seeker = new CasualSeekerFaker().Generate(),
            SubscribedUserIds = [],
        };
        await Silo.CreateGrainAsync<OpenSeekGrain>(0);
        _trackerMock.RemoveSeek(entry.Seeker.UserId, poolKey).Returns(entry);

        await removeStream.OnNextAsync(new OpenSeekRemovedEvent(entry.Seeker.UserId, poolKey));

        await _notifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyOpenSeekEndedAsync(default!, default, default!);
    }
}
