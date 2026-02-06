using System.Numerics;
using System.Runtime.CompilerServices;
using AnarchyChess.EngineShared;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountBits(UInt128 mask) =>
        BitOperations.PopCount((ulong)mask) + BitOperations.PopCount((ulong)(mask >> 64));

    public static UInt128 MaskAdjacent(byte position)
    {
        UInt128 targetBit = UInt128.One << position;
        UInt128 mask = 0;

        mask |= (targetBit & ~BitboardConstants.RightEdgeMask) << 1; // right
        mask |= (targetBit & ~BitboardConstants.LeftEdgeMask) >> 1; // left
        mask |= (targetBit & ~BitboardConstants.TopEdgeMask) << 10; // up
        mask |= (targetBit & ~BitboardConstants.BottomEdgeMask) >> 10; // down
        mask |=
            (targetBit & ~(BitboardConstants.TopEdgeMask | BitboardConstants.RightEdgeMask)) << 11; // up right
        mask |=
            (targetBit & ~(BitboardConstants.TopEdgeMask | BitboardConstants.LeftEdgeMask)) << 9; // up left
        mask |=
            (targetBit & ~(BitboardConstants.BottomEdgeMask | BitboardConstants.RightEdgeMask))
            >> 9; // bottom right
        mask |=
            (targetBit & ~(BitboardConstants.BottomEdgeMask | BitboardConstants.LeftEdgeMask))
            >> 11; // bottom left
        return mask;
    }

    public static void CreateMoveFromAttacks(
        byte from,
        PieceType pieceType,
        BitBoard board,
        UInt128 attacks,
        UInt128 occupancy,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 captures = attacks & occupancy;
        UInt128 quiets = attacks & ~occupancy;

        while (quiets != 0)
        {
            byte toSquare = (byte)BitScanForward(ref quiets);
            moves[moveCount++] = new BitMove
            {
                From = from,
                To = toSquare,
                Piece = pieceType,
            };
        }

        while (captures != 0)
        {
            byte toSquare = (byte)BitScanForward(ref captures);
            if (board.TryGetPieceAt(toSquare, out var capturePiece))
            {
                BitMove move = new()
                {
                    From = from,
                    To = toSquare,
                    Piece = pieceType,
                };
                move.AddCapture(toSquare, capturePiece.Value.PieceType, capturePiece.Value.Color);
                moves[moveCount++] = move;
            }
        }
    }
}
