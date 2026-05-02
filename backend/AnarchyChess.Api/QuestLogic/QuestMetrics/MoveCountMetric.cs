using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.GameLengthMetric")]
public class MoveCountMetric : IQuestMetric
{
    public int Evaluate(GameQuestSnapshot snapshot) => snapshot.Board.Moves.Count / 2;
}
