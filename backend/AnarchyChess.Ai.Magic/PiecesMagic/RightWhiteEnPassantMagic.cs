using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public sealed class RightWhiteEnPassantMagic : PieceMagic
{
    public override string Name => "RightWhiteEnPassant";

    public override UInt128 GenerateMask(int x, int y) =>
        SlideMask(x - 1, y - 2, new Offset(X: 1, Y: 1));

    public override UInt128 ComputeAttacks(int x, int y, UInt128 blocker) =>
        BlockedSlideAttack(x - 1, y - 2, new Offset(X: 1, Y: 1), blocker);
}
