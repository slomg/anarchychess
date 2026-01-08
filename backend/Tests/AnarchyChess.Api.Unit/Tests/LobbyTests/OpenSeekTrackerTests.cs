using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LobbyTests;

public class OpenSeekTrackerTests
{
    private readonly IRandomProvider _randomProvider = Substitute.For<IRandomProvider>();
    private readonly OpenSeekTracker _tracker;

    public OpenSeekTrackerTests()
    {
        _tracker = new(_randomProvider);
        _randomProvider.Next(Arg.Any<int>()).Returns(0);
    }

    [Fact]
    public void Subscribe_adds_a_new_connection_when_the_user_has_no_connections()
    {
        var seeker = new CasualSeekerFaker().Generate();
        ConnectionId connectionId = "conn1";

        var result = _tracker.Subscribe(connectionId, seeker);

        result.Should().BeEmpty();
        var watcherConnIds = _tracker.GetUserConnectionIds(seeker.UserId);
        watcherConnIds.Should().NotBeNull();
        watcherConnIds.Should().ContainSingle().Which.Should().Be(connectionId);
    }

    [Fact]
    public void Subscribe_adds_a_new_connection_when_the_user_already_has_connections()
    {
        var seeker = new CasualSeekerFaker().Generate();
        ConnectionId firstConnection = "conn1";
        ConnectionId secondConnection = "conn2";
        _tracker.Subscribe(firstConnection, seeker);

        var result = _tracker.Subscribe(secondConnection, seeker);

        result.Should().BeEmpty();
        var watcherConnIds = _tracker.GetUserConnectionIds(seeker.UserId);
        watcherConnIds.Should().NotBeNull();
        watcherConnIds.Should().BeEquivalentTo([firstConnection, secondConnection]);
    }

    [Fact]
    public void Subscribe_ignores_incompatible_seekers()
    {
        UserId watcherId = "watcher id";
        UserId watcherExcludedSeekerId = "excluded seeker id";
        var watcher = new CasualSeekerFaker(watcherId)
            .RuleFor(x => x.ExcludeUserIds, [watcherExcludedSeekerId])
            .Generate();
        var excludedSeekerByWatcher = new CasualSeekerFaker(watcherExcludedSeekerId).Generate();
        var seekerExcludingWatcher = new CasualSeekerFaker()
            .RuleFor(x => x.ExcludeUserIds, [watcherId])
            .Generate();
        var compatibleSeeker = new CasualSeekerFaker().Generate();

        _tracker.AddSeek(excludedSeekerByWatcher, new PoolKeyFaker().Generate());
        _tracker.AddSeek(seekerExcludingWatcher, new PoolKeyFaker().Generate());
        _tracker.AddSeek(compatibleSeeker, new PoolKeyFaker().Generate());

        var result = _tracker.Subscribe("conn", watcher);

        result.Should().ContainSingle().Which.UserId.Should().Be(compatibleSeeker.UserId);
    }

    [Fact]
    public void Subscribe_respects_MAX_INITIAL_SEEKS()
    {
        var watcher = new CasualSeekerFaker().Generate();
        var seekers = new CasualSeekerFaker().Generate(OpenSeekTracker.MAX_INITIAL_SEEKS + 5);
        foreach (var seeker in seekers)
        {
            _tracker.AddSeek(seeker, new PoolKeyFaker().Generate());
        }

        _randomProvider.Next(Arg.Any<int>()).Returns(0);

        var result = _tracker.Subscribe("conn", watcher);

        result.Should().HaveCount(OpenSeekTracker.MAX_INITIAL_SEEKS);
    }

    [Fact]
    public void Subscribe_respects_random_selection()
    {
        var watcher = new CasualSeekerFaker().Generate();
        var seekers = new CasualSeekerFaker().Generate(3);
        foreach (var seeker in seekers)
        {
            _tracker.AddSeek(seeker, new PoolKeyFaker().Generate());
        }

        _randomProvider.Next(3).Returns(1); // pick index 1 first
        _randomProvider.Next(2).Returns(0); // pick index 0 next
        _randomProvider.Next(1).Returns(0); // pick index 0 last (only one left)

        var result = _tracker.Subscribe("conn", watcher);

        result.Should().HaveCount(3);
        result[0].UserId.Should().Be(seekers[1].UserId); // first pick = index 1
        result[1].UserId.Should().Be(seekers[0].UserId); // second pick = index 0
        result[2].UserId.Should().Be(seekers[2].UserId); // last pick = remaining user
    }

    [Fact]
    public void Subscribe_interleaves_seeks_from_multiple_users_until_empty()
    {
        var watcher = new CasualSeekerFaker().Generate();

        UserId seeker1UserId = "seeker 1";
        var user1Seeker1 = new CasualSeekerFaker(seeker1UserId).Generate();
        var user1Seeker2 = new CasualSeekerFaker(seeker1UserId).Generate();
        UserId seeker2UserId = "seeker 2";
        var user2Seeker1 = new CasualSeekerFaker(seeker2UserId).Generate();
        var user2Seeker2 = new CasualSeekerFaker(seeker2UserId).Generate();

        _tracker.AddSeek(user1Seeker1, new PoolKeyFaker().Generate());
        _tracker.AddSeek(user1Seeker2, new PoolKeyFaker().Generate());
        _tracker.AddSeek(user2Seeker1, new PoolKeyFaker().Generate());
        _tracker.AddSeek(user2Seeker2, new PoolKeyFaker().Generate());

        var result = _tracker.Subscribe("conn", watcher);

        result.Should().HaveCount(4);

        result[0].UserId.Should().Be(user1Seeker1.UserId);
        result[1].UserId.Should().Be(user2Seeker1.UserId);
        result[2].UserId.Should().Be(user1Seeker2.UserId);
        result[3].UserId.Should().Be(user2Seeker2.UserId);
    }

    [Fact]
    public void Subscribe_does_not_return_a_seek_after_it_has_been_removed()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();
        _tracker.AddSeek(seeker, pool);

        var watcher = new CasualSeekerFaker().Generate();
        _tracker
            .Subscribe("conn", watcher)
            .Should()
            .ContainSingle()
            .Which.UserId.Should()
            .Be(seeker.UserId);

        _tracker.RemoveSeek(seeker.UserId, pool);
        var result = _tracker.Subscribe("conn2", watcher);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Subscribe_returns_the_correct_OpenSeek()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();
        _tracker.AddSeek(seeker, pool);

        var result = _tracker.Subscribe("conn", new CasualSeekerFaker().Generate());

        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new OpenSeek(
                    UserId: seeker.UserId,
                    UserName: seeker.UserName,
                    Pool: pool,
                    Rating: null
                )
            );
    }

    [Fact]
    public void Subscribe_subscribes_the_user_to_the_open_seek()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();
        _tracker.AddSeek(seeker, pool);

        var watcher = new CasualSeekerFaker().Generate();
        _tracker.Subscribe("conn", watcher);

        var result = _tracker.RemoveSeek(seeker.UserId, pool);

        result.Should().NotBeNull();
        result.SubscribedUserIds.Should().BeEquivalentTo([watcher.UserId]);
    }

    [Fact]
    public void Unsubscribe_removes_connection_when_multiple_connections_exist()
    {
        var seeker = new CasualSeekerFaker().Generate();
        ConnectionId firstConnection = "conn1";
        ConnectionId secondConnection = "conn2";
        _tracker.Subscribe(firstConnection, seeker);
        _tracker.Subscribe(secondConnection, seeker);

        _tracker.Unsubscribe(seeker.UserId, firstConnection);

        var watcherConnIds = _tracker.GetUserConnectionIds(seeker.UserId);
        watcherConnIds.Should().ContainSingle().Which.Should().Be(secondConnection);
    }

    [Fact]
    public void Unsubscribe_removes_user_when_last_connection_is_removed()
    {
        var seeker = new CasualSeekerFaker().Generate();
        ConnectionId connection = "conn1";
        _tracker.Subscribe(connection, seeker);

        _tracker.Unsubscribe(seeker.UserId, connection);

        var watcher = _tracker.GetUserConnectionIds(seeker.UserId);
        watcher.Should().BeNull();
    }

    [Fact]
    public void Unsubscribe_does_nothing_if_user_does_not_exist()
    {
        UserId nonExistentUser = "non-existent";

        _tracker.Unsubscribe(nonExistentUser, "conn1");

        _tracker.GetUserConnectionIds(nonExistentUser).Should().BeNull();
    }

    [Fact]
    public void Unsubscribe_does_nothing_if_connection_id_does_not_exist()
    {
        var seeker = new CasualSeekerFaker().Generate();
        ConnectionId existingConnection = "conn1";
        _tracker.Subscribe(existingConnection, seeker);
        ConnectionId nonExistentConnection = "conn2";

        _tracker.Unsubscribe(seeker.UserId, nonExistentConnection);

        var watcherConnIds = _tracker.GetUserConnectionIds(seeker.UserId);
        watcherConnIds.Should().ContainSingle().Which.Should().Be(existingConnection);
    }

    [Fact]
    public void Unsubscribe_removing_one_users_connection_does_not_affect_other_users()
    {
        var user1 = new CasualSeekerFaker().Generate();
        var user2 = new CasualSeekerFaker().Generate();

        ConnectionId user1Conn1 = "user1 conn1";
        ConnectionId user1Conn2 = "user1 conn2";
        ConnectionId user2Conn = "user2 conn1";

        _tracker.Subscribe(user1Conn1, user1);
        _tracker.Subscribe(user1Conn2, user1);
        _tracker.Subscribe(user2Conn, user2);

        _tracker.Unsubscribe(user1.UserId, user1Conn1);

        var watcher1ConnIds = _tracker.GetUserConnectionIds(user1.UserId);
        watcher1ConnIds.Should().ContainSingle().Which.Should().Be(user1Conn2);

        var watcher2ConnIds = _tracker.GetUserConnectionIds(user2.UserId);
        watcher2ConnIds.Should().ContainSingle().Which.Should().Be(user2Conn);
    }

    [Fact]
    public void AddSeek_adds_a_new_seek_for_a_new_user()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();

        var entry = _tracker.AddSeek(seeker, pool);

        entry.Should().NotBeNull();
        entry.Seeker.Should().Be(seeker);
        entry.OpenSeek.UserId.Should().Be(seeker.UserId);
        entry.OpenSeek.Pool.Should().Be(pool);
        entry.SubscribedUserIds.Should().BeEmpty();
    }

    [Fact]
    public void AddSeek_overwrites_existing_seek_for_same_user_and_pool()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();
        _tracker.AddSeek(seeker, pool);

        var newSeekerName = "UpdatedSeeker";
        var updatedSeeker = seeker with { UserName = newSeekerName };

        var entry = _tracker.AddSeek(updatedSeeker, pool);

        entry.Seeker.UserName.Should().Be(newSeekerName);
        entry.OpenSeek.UserName.Should().Be(newSeekerName);
        entry.SubscribedUserIds.Should().BeEmpty();
    }

    [Fact]
    public void AddSeek_populates_subscribed_user_ids_with_compatible_watchers()
    {
        var compatibleWatcher = new CasualSeekerFaker().Generate();
        var incompatibleWatcher = new CasualSeekerFaker().Generate();
        _tracker.Subscribe("conn1", compatibleWatcher);
        _tracker.Subscribe("conn2", incompatibleWatcher);

        var seeker = new CasualSeekerFaker()
            .RuleFor(x => x.ExcludeUserIds, [incompatibleWatcher.UserId])
            .Generate();

        var entry = _tracker.AddSeek(seeker, new PoolKeyFaker().Generate());

        entry
            .SubscribedUserIds.Should()
            .ContainSingle()
            .Which.Should()
            .Be(compatibleWatcher.UserId);
    }

    [Fact]
    public void AddSeek_includes_rating_for_rated_seeker()
    {
        var ratedSeeker = new RatedSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();

        var entry = _tracker.AddSeek(ratedSeeker, pool);

        entry.OpenSeek.Rating.Should().Be(ratedSeeker.Rating.Value);
    }

    [Fact]
    public void RemoveSeek_returns_entry_and_removes_it()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool = new PoolKeyFaker().Generate();

        var addedEntry = _tracker.AddSeek(seeker, pool);

        var removedEntry = _tracker.RemoveSeek(seeker.UserId, pool);

        removedEntry.Should().Be(addedEntry);

        var secondRemove = _tracker.RemoveSeek(seeker.UserId, pool);
        secondRemove.Should().BeNull();
    }

    [Fact]
    public void RemoveSeek_returns_null_if_pool_does_not_exist_for_user()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var existingPool = new PoolKeyFaker().Generate();
        _tracker.AddSeek(seeker, existingPool);

        var nonExistentPool = new PoolKeyFaker().Generate();

        var result = _tracker.RemoveSeek(seeker.UserId, nonExistentPool);

        result.Should().BeNull();
    }

    [Fact]
    public void RemoveSeek_removes_only_specified_pool_for_user()
    {
        var seeker = new CasualSeekerFaker().Generate();
        var pool1 = new PoolKeyFaker().Generate();
        var pool2 = new PoolKeyFaker().Generate();

        _tracker.AddSeek(seeker, pool1);
        _tracker.AddSeek(seeker, pool2);

        var removed = _tracker.RemoveSeek(seeker.UserId, pool1);

        removed.Should().NotBeNull();
        removed.OpenSeek.Pool.Should().Be(pool1);

        var remaining = _tracker.RemoveSeek(seeker.UserId, pool2);
        remaining.Should().NotBeNull();
        remaining.OpenSeek.Pool.Should().Be(pool2);
    }
}
