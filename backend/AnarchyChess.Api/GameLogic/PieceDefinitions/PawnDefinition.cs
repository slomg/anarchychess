using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class PawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.Pawn;

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        var behaviours = GetPawnBehaviours(
            board,
            position,
            movingPiece,
            maxInitialMoveDistance: 3,
            promotesTo: GameLogicConstants.PromotablePieces
        );
        foreach (var behaviour in behaviours)
        {
            yield return behaviour;
        }

        yield return new ThrowingRule();
    }
}
