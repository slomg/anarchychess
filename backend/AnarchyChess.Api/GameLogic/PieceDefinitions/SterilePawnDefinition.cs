using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class SterilePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.SterilePawn;

    private static readonly IReadOnlyCollection<PieceType> _promotesTo =
    [
        .. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen),
        PieceType.SterilePawn,
    ];

    public override IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) =>
        GetPawnBehaviours(
            board,
            position,
            movingPiece,
            maxInitialMoveDistance: 1,
            promotesTo: _promotesTo
        );
}
