using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitAntiqueenDefinition : IBitPieceDefinition
{
    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = BitboardConstants.HorseyMasks[position];
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
