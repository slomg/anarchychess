using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.ProgressiveUniquePromotionsMetric")]
public class ProgressiveUniquePromotionsMetric : IQuestMetric
{
    [Id(0)]
    private readonly HashSet<PieceType> _promotedTo = [];

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        int newPromotions = 0;
        foreach (var move in snapshot.Board.Moves)
        {
            if (move.Piece.Color != snapshot.PlayerColor || !move.PromotesTo.HasValue)
                continue;

            var promotesTo = move.PromotesTo.Value;
            if (_promotedTo.Contains(promotesTo))
                continue;

            _promotedTo.Add(promotesTo);
            newPromotions++;
        }

        return newPromotions;
    }
}
