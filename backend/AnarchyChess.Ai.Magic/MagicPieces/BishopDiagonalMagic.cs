using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.MagicPieces;

public sealed class BishopDiagonalMagic : PieceMagic
{
    public override string Name => "Bishop";

    public override UInt128 GenerateMask(int x, int y) =>
        SlideMask(x, y, new Offset(X: 1, Y: 1))
        | SlideMask(x, y, new Offset(X: 1, Y: -1))
        | SlideMask(x, y, new Offset(X: -1, Y: 1))
        | SlideMask(x, y, new Offset(X: -1, Y: -1));

    public override UInt128 ComputeAttacks(int x, int y, UInt128 blocker) =>
        SlideAttack(x, y, new Offset(X: 1, Y: 1), blocker)
        | SlideAttack(x, y, new Offset(X: 1, Y: -1), blocker)
        | SlideAttack(x, y, new Offset(X: -1, Y: 1), blocker)
        | SlideAttack(x, y, new Offset(X: -1, Y: -1), blocker);
}
