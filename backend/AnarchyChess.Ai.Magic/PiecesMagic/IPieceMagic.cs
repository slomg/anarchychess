using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Magic.PiecesMagic;

public interface IPieceMagic
{
    string Name { get; }

    UInt128 GenerateMask(AlgebraicPoint point);
    UInt128 ComputeAttacks(AlgebraicPoint point, UInt128 blocker);
}
