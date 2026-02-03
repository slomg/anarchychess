using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public interface IPieceDefinition
{
    PieceType Type { get; }

    IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    );
}
