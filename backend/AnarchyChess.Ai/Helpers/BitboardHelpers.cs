using System.Numerics;

namespace AnarchyChess.Ai.Helpers;

public static class BitboardHelpers
{
    public static int BitScanForward(ref UInt128 bitboard)
    {
        if (bitboard == 0)
        {
            throw new InvalidOperationException("Cannot scan forward on an empty bitboard");
        }

        ulong low = (ulong)bitboard;
        if (low != 0)
        {
            int index = BitOperations.TrailingZeroCount(low);
            bitboard &= bitboard - 1;
            return index;
        }
        else
        {
            ulong high = (ulong)(bitboard >> 64);
            int index = 64 + BitOperations.TrailingZeroCount(high);
            bitboard &= bitboard - 1;
            return index;
        }
    }
}
