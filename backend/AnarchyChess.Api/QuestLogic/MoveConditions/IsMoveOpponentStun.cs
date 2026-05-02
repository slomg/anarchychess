using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

public sealed class IsMoveOpponentStun : IMoveCondition
{
    public bool Evaluate(Move move) => move.Stuns.Any(x => x.Piece.Color != move.Piece.Color);
}
