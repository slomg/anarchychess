using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameClockTests
{
    private readonly GameClock _clock;
    private readonly TimeControlSettings _timeControl = new(BaseSeconds: 300, IncrementSeconds: 10);
    private readonly GameClockState _state;

    private readonly GameSettings _settings;
    private readonly GameResultDescriber _gameResultDescriber = new();

    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    public GameClockTests()
    {
        var settings = AppSettingsLoader.LoadAppSettings();
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        _settings = settings.Game;

        _clock = new(Options.Create(settings), _gameResultDescriber, _timeProviderMock);

        _state = _clock.Create(_timeControl);
        _state.Clocks[GameColor.White].IsInGracePeriod = false;
        _state.Clocks[GameColor.White].TimeUntilAbandonMs = null;
        _state.Clocks[GameColor.Black].IsInGracePeriod = false;
        _state.Clocks[GameColor.Black].TimeUntilAbandonMs = null;
    }

    [Fact]
    public void Create_creates_state_correctly()
    {
        var result = _clock.Create(_timeControl);

        result
            .Should()
            .BeEquivalentTo(
                new GameClockState()
                {
                    TimeControl = _timeControl,
                    IsFrozen = false,
                    Clocks = new Dictionary<GameColor, ClockPlayer>
                    {
                        [GameColor.White] = new ClockPlayer
                        {
                            TimeLeftMs = _timeControl.BaseSeconds * 1000,
                            TimeUntilAbandonMs = _settings.FirstMoveGracePeriod.TotalMilliseconds,
                            IsInGracePeriod = true,
                        },
                        [GameColor.Black] = new ClockPlayer
                        {
                            TimeLeftMs = _timeControl.BaseSeconds * 1000,
                            TimeUntilAbandonMs = _settings.FirstMoveGracePeriod.TotalMilliseconds,
                            IsInGracePeriod = true,
                        },
                    },
                    LastUpdatedMs = _fakeNow.ToUnixTimeMilliseconds(),
                }
            );
    }

    [Fact]
    public void ToSnapshot_returns_snapshot_with_correct_values()
    {
        _state.IsFrozen = true;

        var snapshot = _clock.ToSnapshot(_state);

        var whiteClock = _state.Clocks[GameColor.White];
        var blackClock = _state.Clocks[GameColor.Black];
        snapshot
            .Should()
            .BeEquivalentTo(
                new ClockSnapshot(
                    WhiteClock: new(
                        TimeLeftMs: whiteClock.TimeLeftMs,
                        TimeUntilAbandonMs: whiteClock.TimeUntilAbandonMs,
                        IsInGracePeriod: whiteClock.IsInGracePeriod
                    ),
                    BlackClock: new(
                        TimeLeftMs: blackClock.TimeLeftMs,
                        TimeUntilAbandonMs: blackClock.TimeUntilAbandonMs,
                        IsInGracePeriod: blackClock.IsInGracePeriod
                    ),
                    LastUpdated: _state.LastUpdatedMs,
                    ServerTime: _fakeNow.ToUnixTimeMilliseconds(),
                    IsFrozen: true
                )
            );
    }

    [Fact]
    public void CalculateTimeLeftMs_returns_decreased_time_from_time_left()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        result.Should().Be(250_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_returns_decreased_time_from_abandon_timer_if_present()
    {
        _state.Clocks[GameColor.White].TimeUntilAbandonMs = 200_000;
        _state.Clocks[GameColor.White].TimeLeftMs = 1_234_567;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        result.Should().Be(150_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_doesnt_decrease_time_from_time_left_if_not_isTicking()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CalculateTimeLeftMs(GameColor.White, _state, isTicking: false);

        result.Should().Be(300_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_doesnt_decrease_time_from_abandon_timer_if_not_isTicking()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _state.Clocks[GameColor.White].TimeUntilAbandonMs = 1_234_567;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CalculateTimeLeftMs(GameColor.White, _state, isTicking: false);

        result.Should().Be(300_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_doesnt_decrease_time_if_frozen()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _state.IsFrozen = true;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        result.Should().Be(300_000);
    }

    [Fact]
    public void CalculateTimeLeftMs_doesnt_go_bellow_zero()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 50;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(100));

        var result = _clock.CalculateTimeLeftMs(GameColor.White, _state);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateTimeLeftMs_works_with_black()
    {
        _state.Clocks[GameColor.Black].TimeLeftMs = 700_000;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CalculateTimeLeftMs(GameColor.Black, _state);

        result.Should().Be(650_000);
    }

    [Fact]
    public void CommitTurn_updates_time_left_with_increment()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(5_000));

        var result = _clock.CommitTurn(GameColor.White, _state);

        result.Should().Be(305_000); // 300_000 - 5_000 + 10_000
        _state.Clocks[GameColor.White].TimeLeftMs.Should().Be(305_000);
    }

    [Fact]
    public void CommiTurn_doesnt_decrement_time_left_in_grace_period()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _state.Clocks[GameColor.White].IsInGracePeriod = true;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(50_000));

        var result = _clock.CommitTurn(GameColor.White, _state);

        result.Should().Be(300_000);
        _state.Clocks[GameColor.White].TimeLeftMs.Should().Be(300_000);
    }

    [Fact]
    public void CommitTurn_clears_grace_period()
    {
        _state.Clocks[GameColor.White].IsInGracePeriod = true;

        _clock.CommitTurn(GameColor.White, _state);

        _state.Clocks[GameColor.White].IsInGracePeriod.Should().BeFalse();
    }

    [Fact]
    public void CommitTurn_clears_abandon_timer()
    {
        _state.Clocks[GameColor.White].TimeUntilAbandonMs = 50_000;

        _clock.CommitTurn(GameColor.White, _state);

        _state.Clocks[GameColor.White].TimeUntilAbandonMs.Should().BeNull();
    }

    [Fact]
    public void CommitTurn_updates_last_updated_timestamp()
    {
        var newTime = _fakeNow + TimeSpan.FromMilliseconds(1234);
        _timeProviderMock.GetUtcNow().Returns(newTime);

        _clock.CommitTurn(GameColor.White, _state);

        _state.LastUpdatedMs.Should().Be(newTime.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void CommitTurn_does_not_modify_other_players_clock()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _state.Clocks[GameColor.Black].TimeLeftMs = 400_000;

        _clock.CommitTurn(GameColor.White, _state);

        _state.Clocks[GameColor.Black].TimeLeftMs.Should().Be(400_000);
    }

    [Fact]
    public void CommitTurn_works_with_black()
    {
        _state.Clocks[GameColor.Black].TimeLeftMs = 600_000;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(10_000));

        var result = _clock.CommitTurn(GameColor.Black, _state);

        result.Should().Be(600_000);
    }

    [Fact]
    public void CommitTurn_does_not_apply_increment_after_timeout()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 5;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(10));

        var result = _clock.CommitTurn(GameColor.White, _state);

        result.Should().Be(0);
        _state.Clocks[GameColor.White].TimeLeftMs.Should().Be(0);
    }

    [Fact]
    public void CommitLastTurn_freezes_the_clock()
    {
        _clock.CommitLastTurn(GameColor.White, _state);

        _state.IsFrozen.Should().BeTrue();
    }

    [Fact]
    public void CommitLastTurn_does_not_apply_increment()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(5_000));

        _clock.CommitLastTurn(GameColor.White, _state);

        _state.Clocks[GameColor.White].TimeLeftMs.Should().Be(295_000); // 300_000 - 5_000, no increment
    }

    [Fact]
    public void DetectTimeout_returns_null_when_no_player_has_timed_out()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 200_000;
        _state.Clocks[GameColor.Black].TimeLeftMs = 200_000;

        var result = _clock.DetectTimeout(GameColor.White, _state);

        result.Should().BeNull();
    }

    [Fact]
    public void DetectTimeout_returns_white_timeout_when_white_time_runs_out()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 100;
        _state.Clocks[GameColor.Black].TimeLeftMs = 200_000;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(90));

        var result = _clock.DetectTimeout(GameColor.White, _state);

        result.Should().BeEquivalentTo(_gameResultDescriber.Timeout(GameColor.White));
    }

    [Fact]
    public void DetectTimeout_returns_black_timeout_when_black_time_is_exhausted()
    {
        _state.Clocks[GameColor.Black].TimeLeftMs = 100;
        _state.Clocks[GameColor.White].TimeLeftMs = 200_000;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(90));

        var result = _clock.DetectTimeout(GameColor.Black, _state);

        result.Should().BeEquivalentTo(_gameResultDescriber.Timeout(GameColor.Black));
    }

    [Fact]
    public void DetectTimeout_aborts_game_when_player_times_out_in_grace_period()
    {
        _state.Clocks[GameColor.White].IsInGracePeriod = true;
        _state.Clocks[GameColor.White].TimeUntilAbandonMs = 50;
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(100));

        var result = _clock.DetectTimeout(GameColor.White, _state);

        result.Should().BeEquivalentTo(_gameResultDescriber.Aborted(GameColor.White));
    }

    [Fact]
    public void DetectTimeout_abandons_game_when_player_times_out_during_abandon_window()
    {
        _state.Clocks[GameColor.White].IsInGracePeriod = false;
        _state.Clocks[GameColor.White].TimeUntilAbandonMs = 50;
        _state.Clocks[GameColor.White].TimeLeftMs = 300_000;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(100));

        var result = _clock.DetectTimeout(GameColor.White, _state);

        result.Should().BeEquivalentTo(_gameResultDescriber.Abandoned(GameColor.White));
    }

    [Fact]
    public void DetectTimeout_only_considers_ticking_player_for_elapsed_time()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 200_000;
        _state.Clocks[GameColor.Black].TimeLeftMs = 100;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(90));

        var result = _clock.DetectTimeout(GameColor.White, _state);

        result.Should().BeNull();
    }

    [Fact]
    public void IsOvertime_returns_true_for_time_under_10_ms()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 100;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(90));

        var result = _clock.IsOvertime(GameColor.White, isTicking: true, _state);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOvertime_returns_false_for_time_over_10_ms()
    {
        _state.Clocks[GameColor.White].TimeLeftMs = 100;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(89));

        var result = _clock.IsOvertime(GameColor.White, isTicking: true, _state);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsOvertime_only_ticks_when_asked()
    {
        _state.Clocks[GameColor.Black].TimeLeftMs = 100;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromMilliseconds(90));

        var tickingResult = _clock.IsOvertime(GameColor.Black, isTicking: true, _state);
        var notTickingResult = _clock.IsOvertime(GameColor.Black, isTicking: false, _state);

        tickingResult.Should().BeTrue();
        notTickingResult.Should().BeFalse();
    }
}
