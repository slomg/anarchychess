using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitRookDefinition : IBitPieceDefinition
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
        UInt128 attacks = MagicLibrary.GetAttacks(
            MagicLibrary.RookTable,
            position,
            board.Occupancy
        );

        UInt128 horseyAttacks = attacks & board.BitboardFor(PieceType.Horsey, color);
        while (horseyAttacks != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref horseyAttacks);
            BitMove move = new()
            {
                From = position,
                To = toSquare,
                Piece = pieceType,
                PromotesTo = PieceType.Knook,
                SpecialMoveType = SpecialMoveType.KnooklearFusion,
            };
            move.AddCapture(toSquare, PieceType.Horsey, color);

            UInt128 captures = BitboardHelpers.MaskAdjacent(toSquare);
            captures &= board.Occupancy;

            while (captures != 0)
            {
                byte capturedSquare = (byte)BitboardHelpers.BitScanForward(ref captures);
                if (board.TryGetPieceAt(capturedSquare, out var piece))
                {
                    move.AddCapture(capturedSquare, piece.Value.PieceType, piece.Value.Color);
                }
            }

            moves[moveCount++] = move;
        }

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            pieceType,
            board,
            attacks & ~board.BitboardForFriendOf(color),
            board.Occupancy,
            moves,
            ref moveCount
        );
    }
}
