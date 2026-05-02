using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.OwnMoveOccurredCondition")]
public class OwnMoveOccurredCondition(params IMoveCondition[] moveConditions) : IQuestCondition
{
    [Id(0)]
    private readonly IMoveCondition[] _moveConditions = moveConditions;

    public bool Evaluate(GameQuestSnapshot snapshot)
    {
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.Board.Moves.Count; i += 2)
        {
            if (_moveConditions.All(x => x.Evaluate(snapshot.Board.Moves[i])))
                return true;
        }
        return false;
    }
}
