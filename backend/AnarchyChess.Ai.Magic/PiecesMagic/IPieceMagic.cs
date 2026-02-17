namespace AnarchyChess.Ai.Magic.PiecesMagic;

public interface IPieceMagic
{
    string Name { get; }

    UInt128 GenerateMask(int x, int y);
    UInt128 ComputeAttacks(int x, int y, UInt128 blocker);
}
