using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.PieceDefinitions;

public class AntiqueenDefinition : IPieceDefinition
{
    public PieceType Type => PieceType.Antiqueen;

    private readonly IPieceMovementRule[] _behaviours =
    [
        new CaptureRule(
            new StepBehaviour(new Offset(X: 1, Y: 2)),
            new StepBehaviour(new Offset(X: -1, Y: 2)),
            new StepBehaviour(new Offset(X: 1, Y: -2)),
            new StepBehaviour(new Offset(X: -1, Y: -2)),
            new StepBehaviour(new Offset(X: 2, Y: 1)),
            new StepBehaviour(new Offset(X: -2, Y: 1)),
            new StepBehaviour(new Offset(X: 2, Y: -1)),
            new StepBehaviour(new Offset(X: -2, Y: -1))
        ),
    ];

    public IEnumerable<IPieceMovementRule> GetBehaviours(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    ) => _behaviours;
}
