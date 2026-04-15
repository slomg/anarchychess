using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitTraitorRookDefinition : IBitPieceDefinition
{
    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 adjacent = PieceMasks.AdjacentMasks[position];
        UInt128 whiteAdjacent = adjacent & board.WhitePieces & ~board.StunnedPieces;
        UInt128 blackAdjacent = adjacent & board.BlackPieces & ~board.StunnedPieces;

        if (whiteAdjacent == 0 && blackAdjacent == 0)
        {
            return;
        }

        int whiteAdjacentCount = BitboardHelpers.CountBits(whiteAdjacent);
        int blackAdjacentCount = BitboardHelpers.CountBits(blackAdjacent);
        if (whiteAdjacentCount > blackAdjacentCount && !board.IsWhiteToMove)
        {
            return;
        }
        if (blackAdjacentCount > whiteAdjacentCount && board.IsWhiteToMove)
        {
            return;
        }

        UInt128 attacks = MagicLibrary.GetAttacks(
            MagicLibrary.RookTable,
            position,
            board.Occupancy
        );

        if (whiteAdjacentCount > blackAdjacentCount)
        {
            attacks &= ~board.WhitePieces;
        }
        else if (blackAdjacentCount > whiteAdjacentCount)
        {
            attacks &= ~board.BlackPieces;
        }
        else
        {
            attacks &= ~board.Occupancy;
        }

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            piece,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );
    }
}
