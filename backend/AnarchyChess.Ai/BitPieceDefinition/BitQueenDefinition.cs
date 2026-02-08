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
            MagicLibrary.BishopTable,
            position,
            board.Occupancy
        );
        UInt128 attacks = straightAttacks | diagonalAttacks;

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            pieceType,
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
        UInt128 right = (positionBit & BitboardConstants.RightEdgeExcludeMask) << 1;
        UInt128 left = (positionBit & BitboardConstants.LeftEdgeExcludeMask) >> 1;
        UInt128 vertical = isWhite
            ? (positionBit & BitboardConstants.TopEdgeExcludeMask) << 10
            : (positionBit & BitboardConstants.BottomEdgeExcludeMask) >> 10;

        if (right == 0 || left == 0 || vertical == 0)
        {
            return;
        }

        UInt128 betaDecayTargets = (right | left | vertical) & ~board.Occupancy;
        if (betaDecayTargets == 0)
        {
            return;
        }

        BitMove move = new()
        {
            From = position,
            To = position,
            Piece = pieceType,
            CapturesMask = UInt128.One << position,
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
        };
        moves[moveCount++] = move;
    }
}
