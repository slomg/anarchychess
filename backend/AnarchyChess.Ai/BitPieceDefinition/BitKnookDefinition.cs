using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitKnookDefinition : IBitPieceDefinition
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
        UInt128 horseyAttacks = BitboardConstants.HorseyMasks[position];
        UInt128 rookAttacks = MagicLibrary.GetAttacks(
            MagicLibrary.TwoStraightSquaresTable,
            position,
            board.Occupancy
        );

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            pieceType,
            board,
            (horseyAttacks | rookAttacks) & ~board.BitboardForFriendOf(color),
            board.Occupancy,
            moves,
            ref moveCount
        );
    }
}
