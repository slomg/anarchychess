using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public sealed class BlackEnPassantMagic : PieceMagic
{
    public override string Name => "BlackEnPassant";

    public override UInt128 GenerateMask(int x, int y) =>
        SlideMask(x + 1, y, new Offset(X: 1, Y: -1))
        | SlideMask(x - 1, y, new Offset(X: -1, Y: -1));

    public override UInt128 ComputeAttacks(int x, int y, UInt128 blocker) =>
        SlideAttack(x, y, new Offset(X: 1, Y: -1), blocker, attackOffset: new Offset(X: 0, Y: -1))
        | SlideAttack(
            x,
            y,
            new Offset(X: -1, Y: -1),
            blocker,
            attackOffset: new Offset(X: 0, Y: -1)
        );
}
