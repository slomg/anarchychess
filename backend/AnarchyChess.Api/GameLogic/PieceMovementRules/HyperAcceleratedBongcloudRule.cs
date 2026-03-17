using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class HyperAcceleratedBongcloudRule : IPieceMovementRule
{
    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        if (board.Moves.Count > 1)
        {
            yield break;
        }

        int direction = movingPiece.Color is GameColor.White ? 1 : -1;
        AlgebraicPoint to = position + new Offset(X: 0, Y: direction);

        if (
            !board.TryGetPieceAt(to, out var capturingPiece)
            || capturingPiece.Color != movingPiece.Color
        )
        {
            yield break;
        }

        yield return new Move(
            from: position,
            to: to,
            movingPiece,
            captures: [new MoveCapture(capturingPiece, to)]
        );
    }
}
