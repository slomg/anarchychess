using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.OwnMoveCountMetric")]
public class OwnMoveCountMetric(params IMoveCondition[] moveConditions) : IQuestMetric
{
    [Id(0)]
    private readonly IMoveCondition[] _moveConditions = moveConditions;

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        int count = 0;
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.Board.Moves.Count; i += 2)
        {
            if (_moveConditions.All(x => x.Evaluate(snapshot.Board.Moves[i])))
                count++;
        }
        return count;
    }
}
