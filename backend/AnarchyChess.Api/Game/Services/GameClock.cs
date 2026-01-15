using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IGameClock
{
    double CalculateTimeLeftMs(GameColor color, GameClockState state, bool isTicking = true);
    void CommitLastTurn(GameColor color, GameClockState state);
    double CommitTurn(GameColor color, GameClockState state);
    GameColor? DetectTimeout(GameColor tickingPlayer, GameClockState state);
    void Reset(GameClockState state);
    ClockSnapshot ToSnapshot(GameClockState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.GameClockState")]
public class GameClockState
{
    [Id(0)]
    public Dictionary<GameColor, double> ClocksMs { get; init; } =
        new() { [GameColor.White] = 0, [GameColor.Black] = 0 };

    [Id(1)]
    public required TimeControlSettings TimeControl { get; init; }

    [Id(2)]
    public long LastUpdatedMs { get; set; }

    [Id(3)]
    public bool IsFrozen { get; set; }

    [Id(4)]
    public Dictionary<GameColor, bool> HasMadeFirstMove { get; init; } =
        new() { [GameColor.White] = false, [GameColor.Black] = false };
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

    public ClockSnapshot ToSnapshot(GameClockState state) =>
        new(
            WhiteClock: state.ClocksMs[GameColor.White],
            BlackClock: state.ClocksMs[GameColor.Black],
            LastUpdated: state.LastUpdatedMs,
            ServerTime: _timeProvider.GetUtcNow().ToUnixTimeMilliseconds(),
            IsFrozen: state.IsFrozen
        );

    public void Reset(GameClockState state)
    {
        state.ClocksMs[GameColor.White] = _settings.FirstMoveGracePeriod.Milliseconds;
        state.ClocksMs[GameColor.Black] = _settings.FirstMoveGracePeriod.Milliseconds;
        state.HasMadeFirstMove[GameColor.White] = false;
        state.HasMadeFirstMove[GameColor.Black] = false;

        state.LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
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
        if (state.IsFrozen || !isTicking)
            return state.ClocksMs[color];

        var elapsedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastUpdatedMs;
        return Math.Max(0, state.ClocksMs[color] - elapsedMs);
    }

    public GameEndStatus? DetectTimeout(GameColor tickingPlayer, GameClockState state)
    {
        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        foreach (var color in new[] { GameColor.White, GameColor.Black })
        {
            if (!state.HasMadeFirstMove[color])
            {
                var elapsed = nowMs - state.LastUpdatedMs;
                if (elapsed >= _settings.FirstMoveGracePeriod.Milliseconds)
                    return _gameResultDescriber.Aborted(by: color);
            }
        }

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

        GameColor? timedOutColor = null;
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

        return _gameResultDescriber.Timeout(by: timedOutColor.Value);
    }

    private void UpdateTimeLeft(GameColor color, double timeLeft, GameClockState state)
    {
        state.ClocksMs[color] = timeLeft;
        state.LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        if (!state.HasMadeFirstMove[color])
        {
            state.HasMadeFirstMove[color] = true;
        }
    }
}
