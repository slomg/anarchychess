using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public class TwoDiagonalSquaresMagic : PieceMagic
{
    public override string Name => "TwoDiagonalSquares";

    public override UInt128 GenerateMask(AlgebraicPoint point) =>
        SlideMask(point, new Offset(X: 1, Y: 1), limit: 2)
        | SlideMask(point, new Offset(X: 1, Y: -1), limit: 2)
        | SlideMask(point, new Offset(X: -1, Y: 1), limit: 2)
        | SlideMask(point, new Offset(X: -1, Y: -1), limit: 2);

    public override UInt128 ComputeAttacks(AlgebraicPoint point, UInt128 blocker) =>
        SlideAttack(point, new Offset(X: 1, Y: 1), blocker, limit: 2)
        | SlideAttack(point, new Offset(X: 1, Y: -1), blocker, limit: 2)
        | SlideAttack(point, new Offset(X: -1, Y: 1), blocker, limit: 2)
        | SlideAttack(point, new Offset(X: -1, Y: -1), blocker, limit: 2);
}
