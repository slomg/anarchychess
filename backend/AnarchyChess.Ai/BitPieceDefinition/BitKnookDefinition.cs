using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitKnookDefinition : IBitPieceDefinition
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
        UInt128 horseyAttacks = PieceMasks.HorseyMasks[position];
        UInt128 rookAttacks = MagicLibrary.GetAttacks(
            MagicLibrary.TwoStraightSquaresTable,
            position,
            board.Occupancy
        );

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            piece,
            (horseyAttacks | rookAttacks) & ~board.BitboardForFriendOf(piece.Color),
            board.Occupancy,
            moves,
            ref moveCount
        );
    }
}
