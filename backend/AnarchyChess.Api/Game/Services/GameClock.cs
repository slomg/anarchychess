using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IGameClock
{
    ClockSnapshot ToSnapshot(GameClockState state);
    GameClockState Create(TimeControlSettings timeControl);
    double CalculateTimeLeftMs(GameColor color, GameClockState state, bool isTicking = true);
    void CommitLastTurn(GameColor color, GameClockState state);
    double CommitTurn(GameColor color, GameClockState state);
    GameEndStatus? DetectTimeout(GameColor tickingPlayer, GameClockState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.GameClockState")]
public class GameClockState
{
    [Id(0)]
    public required Dictionary<GameColor, ClockPlayer> Clocks { get; init; }

    [Id(1)]
    public required TimeControlSettings TimeControl { get; init; }

    [Id(2)]
    public long LastUpdatedMs { get; set; }

    [Id(3)]
    public bool IsFrozen { get; set; }
}

public class GameClock(
    IOptions<AppSettings> settings,
    IGameResultDescriber gameResultDescriber,
    TimeProvider timeProvider
) : IGameClock
{
    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IGameResultDescriber _gameResultDescriber = gameResultDescriber;
    private readonly TimeProvider _timeProvider = timeProvider;

    public ClockSnapshot ToSnapshot(GameClockState state)
    {
        var whiteClock = state.Clocks[GameColor.White];
        var blackClock = state.Clocks[GameColor.Black];

        return new(
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
            LastUpdated: state.LastUpdatedMs,
            ServerTime: _timeProvider.GetUtcNow().ToUnixTimeMilliseconds(),
            IsFrozen: state.IsFrozen
        );
    }

    public GameClockState Create(TimeControlSettings timeControl)
    {
        var startingTime = timeControl.BaseSeconds * 1000;
        ClockPlayer whiteClock = new()
        {
            TimeLeftMs = startingTime,
            TimeUntilAbandonMs = _settings.FirstMoveGracePeriod.TotalMilliseconds,
            IsInGracePeriod = true,
        };
        ClockPlayer blackClock = new()
        {
            TimeLeftMs = startingTime,
            TimeUntilAbandonMs = _settings.FirstMoveGracePeriod.TotalMilliseconds,
            IsInGracePeriod = true,
        };
        return new()
        {
            TimeControl = timeControl,
            Clocks = new() { [GameColor.White] = whiteClock, [GameColor.Black] = blackClock },
            LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds(),
        };
    }

    public double CommitTurn(GameColor color, GameClockState state)
    {
        var incrementMs = state.TimeControl.IncrementSeconds * 1000;
        var timeLeft = CalculateTimeLeftMs(color, state) + incrementMs;
        UpdateTimeLeft(color, timeLeft, state);

        return timeLeft;
    }

    public void CommitLastTurn(GameColor color, GameClockState state)
    {
        var timeLeft = CalculateTimeLeftMs(color, state);
        UpdateTimeLeft(color, timeLeft, state);
        state.IsFrozen = true;
    }

    public double CalculateTimeLeftMs(GameColor color, GameClockState state, bool isTicking = true)
    {
        var clockPlayer = state.Clocks[color];
        if (state.IsFrozen || !isTicking)
        {
            return clockPlayer.TimeLeftMs;
        }

        var elapsedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastUpdatedMs;
        var timeLeft = clockPlayer.TimeUntilAbandonMs ?? clockPlayer.TimeLeftMs;
        return Math.Max(0, timeLeft - elapsedMs);
    }

    public GameEndStatus? DetectTimeout(GameColor tickingPlayer, GameClockState state)
    {
        var whiteTimeLeftMs = CalculateTimeLeftMs(
            GameColor.White,
            state,
            isTicking: tickingPlayer is GameColor.White
        );
        var blackTimeLeftMs = CalculateTimeLeftMs(
            GameColor.Black,
            state,
            isTicking: tickingPlayer is GameColor.Black
        );

        GameColor timedOutColor;
        if (whiteTimeLeftMs <= 100)
        {
            timedOutColor = GameColor.White;
        }
        else if (blackTimeLeftMs <= 100)
        {
            timedOutColor = GameColor.Black;
        }
        else
        {
            return null;
        }

        var clockPlayer = state.Clocks[timedOutColor];
        if (clockPlayer.IsInGracePeriod)
        {
            return _gameResultDescriber.Aborted(by: timedOutColor);
        }
        else if (clockPlayer.TimeUntilAbandonMs is not null)
        {
            return _gameResultDescriber.Abandoned(by: timedOutColor);
        }
        else
        {
            return _gameResultDescriber.Timeout(by: timedOutColor);
        }
    }

    private void UpdateTimeLeft(GameColor color, double timeLeft, GameClockState state)
    {
        var clockPlayer = state.Clocks[color];
        clockPlayer.IsInGracePeriod = false;
        clockPlayer.TimeLeftMs = timeLeft;
        clockPlayer.TimeUntilAbandonMs = null;

        state.LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
    }
}
