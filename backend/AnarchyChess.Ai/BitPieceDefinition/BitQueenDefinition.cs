using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitQueenDefinition : IBitPieceDefinition
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
        UInt128 straightAttacks = MagicLibrary.GetAttacks(
            MagicLibrary.RookTable,
            position,
            board.Occupancy
        );
        UInt128 diagonalAttacks = MagicLibrary.GetAttacks(
            MagicLibrary.QueenDiagonalTable,
            position,
            board.Occupancy
        );
        UInt128 attacks = straightAttacks | diagonalAttacks;

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            pieceType,
            board,
            attacks & ~board.BitboardForFriendOf(color),
            board.Occupancy,
            moves,
            ref moveCount
        );
        GenerateBetaDecayMove(board, pieceType, color, position, moves, ref moveCount);
    }

    private static void GenerateBetaDecayMove(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        bool isWhite = color is BitPieceColor.White;

        UInt128 positionBit = UInt128.One << position;

        UInt128 requiredMask = ~(
            BitboardConstants.LeftEdgeMask
            | BitboardConstants.RightEdgeMask
            | (isWhite ? BitboardConstants.TopEdgeMask : BitboardConstants.BottomEdgeMask)
        );
        UInt128 betaDecayTargets =
            ((positionBit & requiredMask) << 1) // right
            | ((positionBit & requiredMask) >> 1) // left
            | (
                isWhite
                    ? ((positionBit & requiredMask) << 10) // up
                    : ((positionBit & requiredMask) >> 10) // down
            );
        if (betaDecayTargets == 0)
        {
            return;
        }

        betaDecayTargets &= ~board.Occupancy;
        if (betaDecayTargets == 0)
        {
            return;
        }

        BitMove move = new()
        {
            From = position,
            To = position,
            Piece = pieceType,
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
        };
        move.AddCapture(position, pieceType, color);
        moves[moveCount++] = move;
    }
}
