using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public sealed class RookMagic : PieceMagic
{
    public override string Name => "Rook";

    public override UInt128 GenerateMask(AlgebraicPoint point) =>
        SlideMask(point, new Offset(X: 1, Y: 0))
        | SlideMask(point, new Offset(X: -1, Y: 0))
        | SlideMask(point, new Offset(X: 0, Y: 1))
        | SlideMask(point, new Offset(X: 0, Y: -1));

    public override UInt128 ComputeAttacks(AlgebraicPoint point, UInt128 blocker) =>
        SlideAttack(point, new Offset(X: 1, Y: 0), blocker)
        | SlideAttack(point, new Offset(X: -1, Y: 0), blocker)
        | SlideAttack(point, new Offset(X: 0, Y: 1), blocker)
        | SlideAttack(point, new Offset(X: 0, Y: -1), blocker);
}
