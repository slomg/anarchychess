using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.MovementBehaviours;

public interface IMovementBehaviour
{
    public IEnumerable<AlgebraicPoint> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    );
}
