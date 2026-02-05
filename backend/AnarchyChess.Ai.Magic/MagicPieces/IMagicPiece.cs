namespace AnarchyChess.Ai.Magic.MagicPieces;

public interface IMagicPiece
{
    UInt128 GenerateMask(int x, int y);
    UInt128 ComputeAttacks(int x, int y, UInt128 blocker);
}
