using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitRookDefinition : IBitPieceDefinition
{
    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = MagicLibrary.GetAttacks(
            MagicLibrary.RookTable,
            position,
            board.Occupancy
        );

        UInt128 horseyAttacks = attacks & board.BitboardFor(PieceType.Horsey, piece.Color);
        while (horseyAttacks != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref horseyAttacks);

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
