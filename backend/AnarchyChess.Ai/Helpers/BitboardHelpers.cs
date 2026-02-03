using System.Numerics;

namespace AnarchyChess.Ai.Helpers;

public class BitboardHelpers
{
    public static int BitScanForward(ref UInt128 bitboard)
    {
        ulong low = (ulong)bitboard;
        if (low != 0)
        {
            int index = BitOperations.TrailingZeroCount(low);
            bitboard &= bitboard - 1;
            return index;
        }

        ulong high = (ulong)(bitboard >> 64);
        if (high != 0)
        {
            int index = BitOperations.TrailingZeroCount(high) + 64;
            bitboard &= bitboard - 1;
            return index;
        }

        throw new InvalidOperationException("BitScanForward on empty bitboard");
    }

    public static int PopCount(UInt128 bitboard)
    {
        ulong low = (ulong)bitboard;
        ulong high = (ulong)(bitboard >> 64);
        return BitOperations.PopCount(low) + BitOperations.PopCount(high);
    }
}
