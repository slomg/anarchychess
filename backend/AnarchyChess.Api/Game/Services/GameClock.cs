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
    bool IsOvertime(GameColor player, bool isTicking, GameClockState state);
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

    public double CommitTurn(GameColor color, GameClockState state) =>
        ApplyTurn(color, doIncrement: true, state);

    public void CommitLastTurn(GameColor color, GameClockState state)
    {
        ApplyTurn(color, doIncrement: false, state);
        state.IsFrozen = true;
    }

    public double CalculateTimeLeftMs(GameColor color, GameClockState state, bool isTicking = true)
    {
        var clockPlayer = state.Clocks[color];
        var timeLeft = clockPlayer.TimeUntilAbandonMs ?? clockPlayer.TimeLeftMs;
        return GetEffectiveTimeLeftMs(timeLeft, clockPlayer, state, isTicking);
    }

    public GameEndStatus? DetectTimeout(GameColor tickingPlayer, GameClockState state)
    {
        var isWhiteOvertime = IsOvertime(
            GameColor.White,
            isTicking: tickingPlayer is GameColor.White,
            state
        );
        var isBlackOvertime = IsOvertime(
            GameColor.Black,
            isTicking: tickingPlayer is GameColor.Black,
            state
        );

        GameColor timedOutColor;
        if (isWhiteOvertime)
        {
            timedOutColor = GameColor.White;
        }
        else if (isBlackOvertime)
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

    public bool IsOvertime(GameColor player, bool isTicking, GameClockState state)
    {
        var timeLeftMs = CalculateTimeLeftMs(player, state, isTicking);
        return timeLeftMs <= 10;
    }

    private double GetEffectiveTimeLeftMs(
        double prevTimeLeft,
        ClockPlayer clockPlayer,
        GameClockState state,
        bool isTicking = true
    )
    {
        if (state.IsFrozen || !isTicking)
        {
            return clockPlayer.TimeLeftMs;
        }

        var elapsedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastUpdatedMs;
        return Math.Max(0, prevTimeLeft - elapsedMs);
    }

    private double ApplyTurn(GameColor color, bool doIncrement, GameClockState state)
    {
        var clockPlayer = state.Clocks[color];

        if (!clockPlayer.IsInGracePeriod)
        {
            UpdateClock(clockPlayer, doIncrement, state);
        }
        clockPlayer.IsInGracePeriod = false;
        clockPlayer.TimeUntilAbandonMs = null;

        state.LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return clockPlayer.TimeLeftMs;
    }

    private void UpdateClock(ClockPlayer clockPlayer, bool doIncrement, GameClockState state)
    {
        double timeLeft = GetEffectiveTimeLeftMs(clockPlayer.TimeLeftMs, clockPlayer, state);
        if (timeLeft <= 10)
        {
            clockPlayer.TimeLeftMs = 0;
            return;
        }

        int increment = doIncrement ? state.TimeControl.IncrementSeconds * 1000 : 0;
        clockPlayer.TimeLeftMs =
            GetEffectiveTimeLeftMs(clockPlayer.TimeLeftMs, clockPlayer, state) + increment;
    }
}
