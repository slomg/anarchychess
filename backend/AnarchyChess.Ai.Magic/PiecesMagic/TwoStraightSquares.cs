using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public sealed class TwoStraightSquares : PieceMagic
{
    public override string Name => "TwoStraightSquares";

    public override UInt128 GenerateMask(int x, int y) =>
        SlideMask(x, y, new Offset(X: 1, Y: 0), limit: 2)
        | SlideMask(x, y, new Offset(X: -1, Y: 0), limit: 2)
        | SlideMask(x, y, new Offset(X: 0, Y: 1), limit: 2)
        | SlideMask(x, y, new Offset(X: 0, Y: -1), limit: 2);

    public override UInt128 ComputeAttacks(int x, int y, UInt128 blocker) =>
        SlideAttack(x, y, new Offset(X: 1, Y: 0), blocker, limit: 2)
        | SlideAttack(x, y, new Offset(X: -1, Y: 0), blocker, limit: 2)
        | SlideAttack(x, y, new Offset(X: 0, Y: 1), blocker, limit: 2)
        | SlideAttack(x, y, new Offset(X: 0, Y: -1), blocker, limit: 2);
}
