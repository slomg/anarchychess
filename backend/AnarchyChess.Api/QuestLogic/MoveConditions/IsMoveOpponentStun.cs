using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMoveOpponentStun")]
public sealed class IsMoveOpponentStun : IMoveCondition
{
    public bool Evaluate(Move move) => move.Stuns.Any(x => x.Piece.Color != move.Piece.Color);
}
