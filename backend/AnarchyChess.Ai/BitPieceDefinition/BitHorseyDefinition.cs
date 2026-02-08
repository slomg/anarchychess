using AnarchyChess.Ai.Helpers;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitHorseyDefinition : IBitPieceDefinition
{
    public void GenerateMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = BitboardConstants.HorseyMasks[position];

        UInt128 rookAttacks = attacks & board.BitboardFor(PieceType.Rook, color);
        while (rookAttacks != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref rookAttacks);

            UInt128 captures = BitboardHelpers.MaskAdjacent(toSquare);
            captures &= board.Occupancy;
            captures |= UInt128.One << toSquare;

            moves[moveCount++] = new BitMove()
            {
                From = position,
                To = toSquare,
                Piece = pieceType,
                CapturesMask = captures,
                PromotesTo = PieceType.Knook,
                SpecialMoveType = SpecialMoveType.KnooklearFusion,
            };
        }

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            pieceType,
            attacks & ~board.BitboardForFriendOf(color),
            board.Occupancy,
            moves,
            ref moveCount
        );
    }
}
