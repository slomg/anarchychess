using AnarchyChess.Ai.Helpers;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitPawnDefinition : IBitPieceDefinition
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
        bool hasMoved = (board.HasMoved & (UInt128.One << position)) != 0;

        UInt128 enemyPieces = board.BitboardForEnemyOf(color);

        UInt128 positionBit = UInt128.One << position;
        UInt128 steps = 0;
        UInt128 captures = 0;
        if (color is BitPieceColor.White)
        {
            MaskWhitePawnMoves(
                positionBit,
                empty: board.Empty,
                enemyPieces: enemyPieces,
                hasMoved: hasMoved,
                steps: ref steps,
                captures: ref captures
            );
        }
        else
        {
            MaskBlackPawnMoves(
                positionBit,
                empty: board.Empty,
                enemyPieces: enemyPieces,
                hasMoved: hasMoved,
                steps: ref steps,
                captures: ref captures
            );
        }

        BitboardHelpers.CreateMoveFromQuiets(position, pieceType, steps, moves, ref moveCount);
        BitboardHelpers.CreateMoveFromCaptures(
            position,
            pieceType,
            board,
            captures,
            moves,
            ref moveCount
        );
    }

    private static void MaskWhitePawnMoves(
        UInt128 positionBit,
        UInt128 empty,
        UInt128 enemyPieces,
        bool hasMoved,
        ref UInt128 steps,
        ref UInt128 captures
    )
    {
        captures |= (positionBit & BitboardConstants.TopRightEdgeExcludeMask) << 11 & enemyPieces;
        captures |= (positionBit & BitboardConstants.TopLeftEdgeExcludeMask) << 9 & enemyPieces;

        UInt128 stepPositionBit =
            (positionBit & BitboardConstants.TopEdgeExcludeMask) << 10 & empty;
        steps |= stepPositionBit;
        if (hasMoved)
        {
            return;
        }

        stepPositionBit = (stepPositionBit & BitboardConstants.TopEdgeExcludeMask) << 10 & empty;
        steps |= stepPositionBit;
        stepPositionBit = (stepPositionBit & BitboardConstants.TopEdgeExcludeMask) << 10 & empty;
        steps |= stepPositionBit;
    }

    private static void MaskBlackPawnMoves(
        UInt128 positionBit,
        UInt128 empty,
        UInt128 enemyPieces,
        bool hasMoved,
        ref UInt128 steps,
        ref UInt128 captures
    )
    {
        captures |=
            (positionBit & BitboardConstants.BottomRightEdgeExcludeMask) >> 11 & enemyPieces;
        captures |= (positionBit & BitboardConstants.BottomLeftEdgeExcludeMask) >> 9 & enemyPieces;

        UInt128 stepPositionBit =
            (positionBit & BitboardConstants.BottomEdgeExcludeMask) >> 10 & empty;
        steps |= stepPositionBit;
        if (hasMoved)
        {
            return;
        }

        stepPositionBit = (stepPositionBit & BitboardConstants.BottomEdgeExcludeMask) >> 10 & empty;
        steps |= stepPositionBit;
        stepPositionBit = (stepPositionBit & BitboardConstants.BottomEdgeExcludeMask) >> 10 & empty;
        steps |= stepPositionBit;
    }
}
