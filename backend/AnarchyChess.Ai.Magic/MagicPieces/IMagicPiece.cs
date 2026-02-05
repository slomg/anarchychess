namespace AnarchyChess.Ai.Magic.MagicPieces;

public interface IMagicPiece
{
    string Name { get; }

    UInt128 GenerateMask(int x, int y);
    UInt128 ComputeAttacks(int x, int y, UInt128 blocker);
}
