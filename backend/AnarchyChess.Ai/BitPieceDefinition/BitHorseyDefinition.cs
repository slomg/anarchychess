using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitHorseyDefinition : IBitPieceDefinition
{
    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        ref UInt128 seenThrows,
        int depth,
        int maxDepth,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = PieceMasks.HorseyMasks[position];

        UInt128 rookAttacks = attacks & board.BitboardFor(PieceType.Rook, piece.Color);
        while (rookAttacks != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref rookAttacks);

            UInt128 captures = PieceMasks.AdjacentMasks[toSquare];
            captures &= board.Occupancy;
            captures |= UInt128.One << toSquare;

            moves[moveCount++] = new BitMove()
            {
                From = position,
                To = toSquare,
                Piece = piece,
                CapturesMask = captures,
                PromotesTo = PieceType.Knook,
                SpecialMoveType = SpecialMoveType.KnooklearFusion,
            };
        }

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            piece,
            attacks & ~board.BitboardForFriendOf(piece.Color),
            board.Occupancy,
            moves,
            ref moveCount
        );
    }
}
