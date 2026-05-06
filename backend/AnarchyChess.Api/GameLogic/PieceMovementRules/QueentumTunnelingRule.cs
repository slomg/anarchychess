using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class QueentumTunnelingRule(PieceType tunnelWith) : IPieceMovementRule
{
    private readonly PieceType _tunnelWith = tunnelWith;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        var tunnelWith = board.GetAllPiecesWith(type: _tunnelWith, color: movingPiece.Color);
        foreach (var tunnelPiece in tunnelWith)
        {
            if (board.StunnedPieces.ContainsKey(tunnelPiece.Position))
            {
                continue;
            }

            yield return new Move(
                from: position,
                to: tunnelPiece.Position,
                piece: movingPiece,
                sideEffects:
                [
                    new MoveSideEffect(
                        From: tunnelPiece.Position,
                        To: position,
                        Piece: tunnelPiece.Piece
                    ),
                ],
                specialMoveType: SpecialMoveType.QueentumTunnel
            );
        }
    }
}
