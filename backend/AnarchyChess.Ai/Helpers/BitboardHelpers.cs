using System.Numerics;
using System.Runtime.CompilerServices;

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
        int index;

        if (low != 0)
        {
            index = BitOperations.TrailingZeroCount(low);
        }
        else
        {
            ulong high = (ulong)(bitboard >> 64);
            index = 64 + BitOperations.TrailingZeroCount(high);
        }

        bitboard &= bitboard - 1;
        return index;
    }

    public static int BitScanBackward(ref UInt128 bitboard)
    {
        if (bitboard == 0)
        {
            throw new InvalidOperationException("Cannot scan backward on an empty bitboard");
        }

        ulong high = (ulong)(bitboard >> 64);
        int index;

        if (high != 0)
        {
            index = 64 + BitOperations.Log2(high);
        }
        else
        {
            ulong low = (ulong)bitboard;
            index = BitOperations.Log2(low);
        }

        bitboard &= ~(UInt128.One << index);
        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountBits(UInt128 mask) =>
        BitOperations.PopCount((ulong)mask) + BitOperations.PopCount((ulong)(mask >> 64));

    public static UInt128 MaskAdjacent(byte position)
    {
        UInt128 targetBit = UInt128.One << position;
        UInt128 mask = 0;

        mask |= (targetBit & BitboardConstants.RightEdgeExcludeMask) << 1; // right
        mask |= (targetBit & BitboardConstants.LeftEdgeExcludeMask) >> 1; // left
        mask |= (targetBit & BitboardConstants.TopEdgeExcludeMask) << 10; // up
        mask |= (targetBit & BitboardConstants.BottomEdgeExcludeMask) >> 10; // down
        mask |= (targetBit & BitboardConstants.TopRightEdgeExcludeMask) << 11; // up right
        mask |= (targetBit & BitboardConstants.TopLeftEdgeExcludeMask) << 9; // up left
        mask |= (targetBit & BitboardConstants.BottomRightEdgeExcludeMask) >> 9; // bottom right
        mask |= (targetBit & BitboardConstants.BottomLeftEdgeExcludeMask) >> 11; // bottom left
        return mask;
    }

    public static void CreateMoveFromAttacks(
        byte from,
        BitPiece piece,
        UInt128 attacks,
        UInt128 occupancy,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 captures = attacks & occupancy;
        UInt128 quiets = attacks & ~occupancy;

        CreateMoveFromQuiets(from, piece, quiets, moves, ref moveCount);
        CreateMoveFromCaptures(from, piece, captures, moves, ref moveCount);
    }

    public static void CreateMoveFromQuiets(
        byte from,
        BitPiece piece,
        UInt128 quiets,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        while (quiets != 0)
        {
            byte toSquare = (byte)BitScanForward(ref quiets);
            moves[moveCount++] = new BitMove
            {
                From = from,
                To = toSquare,
                Piece = piece,
            };
        }
    }

    public static void CreateMoveFromCaptures(
        byte from,
        BitPiece piece,
        UInt128 captures,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        while (captures != 0)
        {
            byte toSquare = (byte)BitScanForward(ref captures);

            BitMove move = new()
            {
                From = from,
                To = toSquare,
                Piece = piece,
                CapturesMask = UInt128.One << toSquare,
            };
            moves[moveCount++] = move;
        }
    }
}
