using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public sealed class BishopMagic : PieceMagic
{
    public override string Name => "Bishop";

    public override UInt128 GenerateMask(AlgebraicPoint point) =>
        SlideMask(point, new Offset(X: 1, Y: 1))
        | SlideMask(point, new Offset(X: 1, Y: -1))
        | SlideMask(point, new Offset(X: -1, Y: 1))
        | SlideMask(point, new Offset(X: -1, Y: -1));

    public override UInt128 ComputeAttacks(AlgebraicPoint point, UInt128 blocker) =>
        SlideAttack(point, new Offset(X: 1, Y: 1), blocker)
        | SlideAttack(point, new Offset(X: 1, Y: -1), blocker)
        | SlideAttack(point, new Offset(X: -1, Y: 1), blocker)
        | SlideAttack(point, new Offset(X: -1, Y: -1), blocker);
}
