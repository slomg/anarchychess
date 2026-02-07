using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
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
        bool isWhite = color is BitPieceColor.White;

        UInt128 enemyPieces = board.BitboardForEnemyOf(color);

        UInt128 positionBit = UInt128.One << position;
        UInt128 steps = 0;
        UInt128 captures = 0;
        UInt128 promotionEdgeMask;
        if (isWhite)
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

        GenerateRegularPromotionMoves(
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

        GenerateEnPassantMoves(
            pieceType,
            isWhite: isWhite,
            position,
            positionBit: positionBit,
            board,
            enemyPieces: enemyPieces,
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

    private static void GenerateEnPassantMoves(
        PieceType pieceType,
        bool isWhite,
        byte position,
        UInt128 positionBit,
        BitBoard board,
        UInt128 enemyPieces,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        MagicPieceTable magicTable;
        UInt128 enPassantSquares;
        int stepOffset;

        if (isWhite)
        {
            enPassantSquares = board.EnPassantSquares & positionBit << 11;
            magicTable =
                enPassantSquares == 0
                    ? MagicLibrary.LeftWhiteEnPassantTable
                    : MagicLibrary.RightWhiteEnPassantTable;
            enPassantSquares |= board.EnPassantSquares & positionBit << 9;
            stepOffset = 10;
        }
        else
        {
            enPassantSquares = board.EnPassantSquares & positionBit >> 9;
            magicTable =
                enPassantSquares == 0
                    ? MagicLibrary.LeftBlackEnPassantTable
                    : MagicLibrary.RightBlackEnPassantTable;
            enPassantSquares |= board.EnPassantSquares & positionBit >> 11;
            stepOffset = -10;
        }

        while (enPassantSquares != 0)
        {
            byte enPassantSquare = (byte)BitboardHelpers.BitScanForward(ref enPassantSquares);

            UInt128 enPassantCaptures = MagicLibrary.GetAttacks(
                magicTable,
                enPassantSquare,
                occupancy: enemyPieces
            );
            enPassantCaptures &= enemyPieces;

            BitMove move = default;
            while (enPassantCaptures != 0)
            {
                byte toSquare;
                byte captureSquare;
                if (isWhite)
                {
                    captureSquare = (byte)BitboardHelpers.BitScanForward(ref enPassantCaptures);
                }
                else
                {
                    captureSquare = (byte)BitboardHelpers.BitScanBackward(ref enPassantCaptures);
                }
                toSquare = (byte)(captureSquare + stepOffset);

                var capturePiece = board.GetPieceAt(captureSquare);
                move.AddCapture(captureSquare, capturePiece.PieceType, capturePiece.Color);

                BitMove currentMove = move;
                currentMove.From = position;
                currentMove.To = toSquare;
                currentMove.Piece = pieceType;
                currentMove.SpecialMoveType = SpecialMoveType.EnPassant;
                currentMove.ForcedMovePriority = ForcedMovePriority.EnPassant;
                moves[moveCount++] = currentMove;
            }
        }
    }

    private static void GenerateRegularPromotionMoves(
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

            var capturePiece = board.GetPieceAt(toSquare);
            foreach (PieceType piece in PromoteTo)
            {
                BitMove move = new()
                {
                    From = position,
                    To = toSquare,
                    Piece = pieceType,
                    PromotesTo = piece,
                };
                move.AddCapture(toSquare, capturePiece.PieceType, capturePiece.Color);
                moves[moveCount++] = move;
            }
        }
    }
}
