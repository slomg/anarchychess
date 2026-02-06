using AnarchyChess.Ai.Helpers;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitAntiqueenDefinition : IBitPieceDefinition
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
