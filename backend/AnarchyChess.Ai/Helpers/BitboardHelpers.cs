using System.Numerics;
using System.Runtime.CompilerServices;
using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.Helpers;

public static class BitboardHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
