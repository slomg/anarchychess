using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class UnderagePawnDefinition : BasePawnDefinition
{
    public override PieceType Type => PieceType.UnderagePawn;

    private static readonly IReadOnlyCollection<PieceType> _promotesTo =
    [
        .. GameLogicConstants.PromotablePieces,
        PieceType.UnderagePawn,
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
            maxInitialMoveDistance: 2,
            promotesTo: _promotesTo
        );
}
