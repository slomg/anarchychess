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
            BitMove move = new()
            {
                From = position,
                To = toSquare,
                Piece = pieceType,
                SpecialMoveType = SpecialMoveType.KnooklearFusion,
            };
            move.AddCapture(toSquare, PieceType.Rook, color);

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
