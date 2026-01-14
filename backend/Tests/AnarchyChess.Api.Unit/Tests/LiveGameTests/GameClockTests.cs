using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameClockTests
{
    private readonly GameClock _clock;
    private readonly GameClockState _state = new()
    {
        TimeControl = new(BaseSeconds: 300, IncrementSeconds: 10),
    };

    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    public GameClockTests()
    {
        _clock = new(_timeProviderMock);
    }

    [Fact]
    public void Reset_sets_clocks_to_base_seconds_and_updates_last_updated()
    {
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(now);

        _clock.Reset(_state);

        _state.ClocksMs[GameColor.White].Should().Be(_state.TimeControl.BaseSeconds * 1000);
        _state.ClocksMs[GameColor.Black].Should().Be(_state.TimeControl.BaseSeconds * 1000);
        _state.LastUpdatedMs.Should().Be(now.ToUnixTimeMilliseconds());
        _state.IsFrozen.Should().BeFalse();
    }

    [Fact]
    public void CommitTurn_updates_clock_with_elapsed_and_increment()
    {
        _state.ClocksMs[GameColor.White] = 120_000;
        _state.ClocksMs[GameColor.Black] = 120_000;
        _state.TimeControl = new TimeControlSettings(BaseSeconds: 120, IncrementSeconds: 10);
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        int elapsed = 5_000;
        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + elapsed);
        _timeProviderMock.GetUtcNow().Returns(now);

        var result = _clock.CommitTurn(GameColor.White, _state);

        result.Should().Be(125_000); // 120000 - 5000 + 10000
        _state.ClocksMs[GameColor.White].Should().Be(125_000);
        _state.LastUpdatedMs.Should().Be(now.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void CalculateTimeLeftMs_returns_clock_minus_elapsed_when_not_frozen()
    {
        _state.ClocksMs[GameColor.White] = 90_000;
        _state.ClocksMs[GameColor.Black] = 90_000;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        int elapsed = 15_000;
        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + elapsed);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        timeLeft.Should().Be(75_000); // 90000 - 15000
    }

    [Fact]
    public void CalculateTimeLeftMs_does_not_decrease_when_frozen()
    {
        _state.ClocksMs[GameColor.White] = 50_000;
        _state.IsFrozen = true;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Even if time has passed, frozen clock should not decrease
        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 10_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        timeLeft.Should().Be(50_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_does_not_decrease_when_isTicking_false()
    {
        _state.ClocksMs[GameColor.White] = 120_000;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 10_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeftMs(GameColor.White, _state, isTicking: false);

        timeLeft.Should().Be(120_000);
    }

    [Fact]
    public void ToSnapshot_returns_snapshot_with_correct_values()
    {
        _state.ClocksMs[GameColor.White] = 50_000;
        _state.ClocksMs[GameColor.Black] = 60_000;
        _state.LastUpdatedMs = 1234567890;
        _state.IsFrozen = true;

        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(now);

        var snapshot = _clock.ToSnapshot(_state);

        snapshot
            .Should()
            .BeEquivalentTo(
                new ClockSnapshot(
                    WhiteClock: 50_000,
                    BlackClock: 60_000,
                    LastUpdated: 1234567890,
                    ServerTime: now.ToUnixTimeMilliseconds(),
                    IsFrozen: true
                )
            );
    }

    [Fact]
    public void CommitTurn_does_not_affect_opponent_clock()
    {
        _state.ClocksMs[GameColor.White] = 120_000;
        _state.ClocksMs[GameColor.Black] = 120_000;
        _state.TimeControl = new TimeControlSettings(BaseSeconds: 120, IncrementSeconds: 10);
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 3_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        _clock.CommitTurn(GameColor.White, _state);

        _state.ClocksMs[GameColor.Black].Should().Be(120_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_returns_zero_if_elapsed_exceeds_clock()
    {
        _state.ClocksMs[GameColor.White] = 5_000;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 10_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        var timeLeft = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        timeLeft.Should().Be(0);
    }

    [Fact]
    public void CommitLastTurn_freezes_clock_and_updates_time()
    {
        _state.ClocksMs[GameColor.White] = 100_000;
        _state.TimeControl = new TimeControlSettings(BaseSeconds: 100, IncrementSeconds: 10);
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 2_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        _clock.CommitLastTurn(GameColor.White, _state);

        _state.IsFrozen.Should().BeTrue();
        _state.ClocksMs[GameColor.White].Should().Be(108_000); // 100000 - 2000 + 10000
        _state.LastUpdatedMs.Should().Be(now.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void DetectTimeout_returns_null_if_no_player_is_timed_out()
    {
        _state.ClocksMs[GameColor.White] = 50_000;
        _state.ClocksMs[GameColor.Black] = 60_000;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 1_000);
        _timeProviderMock.GetUtcNow().Returns(now);

        var result = _clock.DetectTimeout(GameColor.White, _state);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(50, 101, GameColor.White)]
    [InlineData(101, 50, GameColor.Black)]
    public void DetectTimeout_returns_the_correct_timed_out_color(
        int whiteClock,
        int blackClock,
        GameColor tickingPlayer
    )
    {
        _state.ClocksMs[GameColor.White] = whiteClock;
        _state.ClocksMs[GameColor.Black] = blackClock;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs);
        _timeProviderMock.GetUtcNow().Returns(now);

        var tickingColorResult = _clock.DetectTimeout(tickingPlayer, _state);
        var otherColorResult = _clock.DetectTimeout(tickingPlayer.Invert(), _state);

        tickingColorResult.Should().Be(tickingPlayer);
        otherColorResult.Should().Be(tickingPlayer);
    }

    [Fact]
    public void DetectTimeout_only_ticks_ticking_player()
    {
        _state.ClocksMs[GameColor.White] = 101;
        _state.ClocksMs[GameColor.Black] = 500;
        _state.LastUpdatedMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _state.IsFrozen = false;

        var now = DateTimeOffset.FromUnixTimeMilliseconds(_state.LastUpdatedMs + 450);
        _timeProviderMock.GetUtcNow().Returns(now);

        var whiteResult = _clock.DetectTimeout(GameColor.White, _state);
        whiteResult.Should().Be(GameColor.White);

        var blackResult = _clock.DetectTimeout(GameColor.Black, _state);
        blackResult.Should().Be(GameColor.Black);
    }
}
