using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IGameClock
{
    double CalculateTimeLeftMs(GameColor color, GameClockState state, bool isActivePlayer = true);
    void CommitLastTurn(GameColor color, GameClockState state);
    double CommitTurn(GameColor color, GameClockState state);
    void Reset(GameClockState state);
    ClockSnapshot ToSnapshot(GameClockState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.GameClockState")]
public class GameClockState
{
    [Id(0)]
    public Dictionary<GameColor, double> ClocksMs { get; set; } =
        new() { [GameColor.White] = 0, [GameColor.Black] = 0 };

    [Id(1)]
    public required TimeControlSettings TimeControl { get; set; }

    [Id(2)]
    public long LastUpdatedMs { get; set; }

    [Id(3)]
    public bool IsFrozen { get; set; }
}

public class GameClock(TimeProvider timeProvider) : IGameClock
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public ClockSnapshot ToSnapshot(GameClockState state) =>
        new(
            state.ClocksMs[GameColor.White],
            state.ClocksMs[GameColor.Black],
            state.LastUpdatedMs,
            state.IsFrozen
        );

    public void Reset(GameClockState state)
    {
        state.ClocksMs[GameColor.White] = state.TimeControl.BaseSeconds * 1000;
        state.ClocksMs[GameColor.Black] = state.TimeControl.BaseSeconds * 1000;
        state.LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
    }

    public double CommitTurn(GameColor color, GameClockState state)
    {
        var timeLeft =
            CalculateTimeLeftMs(color, state) + state.TimeControl.IncrementSeconds * 1000;
        state.ClocksMs[color] = timeLeft;
        state.LastUpdatedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return timeLeft;
    }

    public double CalculateTimeLeftMs(
        GameColor color,
        GameClockState state,
        bool isActivePlayer = true
    )
    {
        if (state.IsFrozen || !isActivePlayer)
            return state.ClocksMs[color];

        var elapsedMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastUpdatedMs;
        return state.ClocksMs[color] - elapsedMs;
    }

    public void CommitLastTurn(GameColor color, GameClockState state)
    {
        CommitTurn(color, state);
        state.IsFrozen = true;
    }
}
