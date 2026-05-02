using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.OwnFirstMoveIsCondition")]
public sealed class OwnFirstMoveIsCondition(params IMoveCondition[] moveConditions)
    : IQuestCondition
{
    [Id(0)]
    private readonly IMoveCondition[] _moveConditions = moveConditions;

    public bool Evaluate(GameQuestSnapshot snapshot)
    {
        int firstMoveIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        if (firstMoveIdx >= snapshot.Board.Moves.Count)
        {
            return false;
        }

        Move firstMove = snapshot.Board.Moves[firstMoveIdx];
        return _moveConditions.All(x => x.Evaluate(firstMove));
    }
}
