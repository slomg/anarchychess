using AnarchyChess.Ai.Helpers;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitPawnDefinition : IBitPieceDefinition
{
    private static readonly PieceType[] PromoteTo =
    [
        PieceType.Queen,
        PieceType.Rook,
        PieceType.Bishop,
        PieceType.Horsey,
        PieceType.Knook,
        PieceType.Antiqueen,
        PieceType.Checker,
    ];

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
        UInt128 promotionEdgeMask;
        if (color is BitPieceColor.White)
        {
            promotionEdgeMask = BitboardConstants.TopEdgeMask;
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
            promotionEdgeMask = BitboardConstants.BottomEdgeMask;
            MaskBlackPawnMoves(
                positionBit,
                empty: board.Empty,
                enemyPieces: enemyPieces,
                hasMoved: hasMoved,
                steps: ref steps,
                captures: ref captures
            );
        }

        GeneratePromotionMoves(
            pieceType,
            position,
            steps: ref steps,
            captures: ref captures,
            board,
            promotionEdgeMask: promotionEdgeMask,
            positionBit: positionBit,
            moves,
            ref moveCount
        );

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

    private static void GeneratePromotionMoves(
        PieceType pieceType,
        byte position,
        ref UInt128 steps,
        ref UInt128 captures,
        BitBoard board,
        UInt128 promotionEdgeMask,
        UInt128 positionBit,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 stepPromotions = steps & promotionEdgeMask;
        stepPromotions |= promotionEdgeMask & positionBit;
        steps &= ~promotionEdgeMask;

        while (stepPromotions != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref stepPromotions);

            foreach (PieceType piece in PromoteTo)
            {
                moves[moveCount++] = new BitMove()
                {
                    From = position,
                    To = toSquare,
                    Piece = pieceType,
                    PromotesTo = piece,
                };
            }
        }

        UInt128 capturePromotions = captures & promotionEdgeMask;
        captures &= ~promotionEdgeMask;

        while (capturePromotions != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref capturePromotions);
            if (!board.TryGetPieceAt(toSquare, out var capturePiece))
            {
                continue;
            }

            foreach (PieceType piece in PromoteTo)
            {
                BitMove move = new()
                {
                    From = position,
                    To = toSquare,
                    Piece = pieceType,
                    PromotesTo = piece,
                };
                move.AddCapture(toSquare, capturePiece.Value.PieceType, capturePiece.Value.Color);
                moves[moveCount++] = move;
            }
        }
    }
}
