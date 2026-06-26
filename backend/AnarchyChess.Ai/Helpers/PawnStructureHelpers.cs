namespace AnarchyChess.Ai.Helpers;

public sealed class PawnStructureHelpers
{
    public static UInt128 GetWhitePassed(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 mask = enemyPawns;
        for (int file = 0; file < 10; file++)
        {
            mask |= BitboardHelpers.ShiftDown(mask);
        }
        mask &= (UInt128.One << 100) - 1;

        UInt128 leftMask = BitboardHelpers.ShiftLeft(mask);
        UInt128 rightMask = BitboardHelpers.ShiftRight(mask);

        UInt128 blocking = mask | leftMask | rightMask;
        return pawns & ~blocking;
    }

    public static UInt128 GetBlackPassed(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 mask = enemyPawns;
        for (int file = 0; file < 10; file++)
        {
            mask |= BitboardHelpers.ShiftUpUnmasked(mask);
        }
        mask &= (UInt128.One << 100) - 1;

        UInt128 leftMask = BitboardHelpers.ShiftLeft(mask);
        UInt128 rightMask = BitboardHelpers.ShiftRight(mask);

        UInt128 blocking = mask | leftMask | rightMask;
        return pawns & ~blocking;
    }
}
