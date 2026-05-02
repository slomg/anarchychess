using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.TimeUsedMsMetric")]
public class TimeUsedMsMetric : IQuestMetric
{
    public int Evaluate(GameQuestSnapshot snapshot)
    {
        if (snapshot.Pool is null || snapshot.Clocks is null)
        {
            return -1;
        }
        var timeControl = snapshot.Pool.TimeControl;

        double timeLeft = snapshot.PlayerColor.Match(
            whenWhite: snapshot.Clocks.WhiteClock.TimeLeftMs,
            whenBlack: snapshot.Clocks.BlackClock.TimeLeftMs
        );
        int playerMoves = snapshot.PlayerColor.Match(
            whenWhite: snapshot.Board.Moves.Count / 2,
            whenBlack: (snapshot.Board.Moves.Count + 1) / 2
        );

        double timeUsedMs = timeControl.BaseSeconds * 1000 - timeLeft;
        timeUsedMs += timeControl.IncrementSeconds * 1000 * playerMoves;

        return (int)timeUsedMs;
    }
}
