namespace AnarchyChess.Ai;

public class BitboardHelpers
{
    public static int BitScanForward(ref UInt128 bitboard)
    {
        UInt128 leastSignificantBit = bitboard & (~bitboard + 1);
        int index = PopCount(leastSignificantBit - 1);
        bitboard &= bitboard - 1;
        return index;
    }

    public static int PopCount(UInt128 bitboard)
    {
        int count = 0;
        while (bitboard != 0)
        {
            bitboard &= bitboard - 1;
            count++;
        }
        return count;
    }
}
