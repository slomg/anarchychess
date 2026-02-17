using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitPawnLikeDefinition(PieceType[] promotesTo, int maxInitialSteps)
    : IBitPieceDefinition
{
    private readonly PieceType[] _promoteTo = promotesTo;
    private readonly int _maxInitialSteps = maxInitialSteps;

    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 positionBit = UInt128.One << position;
        bool hasMoved = board.HasPieceMoved(positionBit);
        UInt128 enemyPieces = board.BitboardForEnemyOf(piece.Color);

        UInt128 promotionEdgeMask;
        if (piece.Color is BitPieceColor.White)
        {
            promotionEdgeMask = BitboardConstants.TopEdgeMask;
            GenerateRegularWhitePawnMoves(
                position,
                piece,
                positionBit: positionBit,
                empty: board.Empty,
                enemyPieces: enemyPieces,
                promotionEdgeMask: promotionEdgeMask,
                hasMoved: hasMoved,
                moves,
                ref moveCount
            );
        }
        else
        {
            promotionEdgeMask = BitboardConstants.BottomEdgeMask;
            GenerateRegularBlackPawnMoves(
                position,
                piece,
                positionBit: positionBit,
                empty: board.Empty,
                enemyPieces: enemyPieces,
                promotionEdgeMask: promotionEdgeMask,
                hasMoved: hasMoved,
                moves,
                ref moveCount
            );
        }

        GenerateSelfPromotionIfNeeded(
            piece,
            position,
            positionBit: positionBit,
            promotionEdgeMask: promotionEdgeMask,
            moves,
            ref moveCount
        );

        GenerateEnPassantMoves(
            piece,
            position,
            positionBit: positionBit,
            board,
            enemyPieces: enemyPieces,
            promotionEdgeMask: promotionEdgeMask,
            moves,
            ref moveCount
        );
    }

    private void GenerateRegularWhitePawnMoves(
        byte position,
        BitPiece piece,
        UInt128 positionBit,
        UInt128 empty,
        UInt128 enemyPieces,
        UInt128 promotionEdgeMask,
        bool hasMoved,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 captureLeftMask =
            (positionBit & BitboardConstants.TopRightEdgeExcludeMask) << 11 & enemyPieces;
        if (captureLeftMask != 0)
        {
            GenerateRegularCapturePromotionMove(
                piece,
                from: position,
                to: (byte)(position + 11),
                captureMask: captureLeftMask,
                promotionEdgeMask: promotionEdgeMask,
                destBit: captureLeftMask,
                moves,
                ref moveCount
            );
        }

        UInt128 captureRightMask =
            (positionBit & BitboardConstants.TopLeftEdgeExcludeMask) << 9 & enemyPieces;
        if (captureRightMask != 0)
        {
            GenerateRegularCapturePromotionMove(
                piece,
                from: position,
                to: (byte)(position + 9),
                captureMask: captureRightMask,
                promotionEdgeMask: promotionEdgeMask,
                destBit: captureRightMask,
                moves,
                ref moveCount
            );
        }

        UInt128 stepPositionMask =
            (positionBit & BitboardConstants.TopEdgeExcludeMask) << 10 & empty;
        if (stepPositionMask == 0)
        {
            return;
        }

        byte dest = (byte)(position + 10);
        GenerateRegularStepPromotionMove(
            piece,
            from: position,
            to: dest,
            promotionEdgeMask: promotionEdgeMask,
            destBit: stepPositionMask,
            moves,
            ref moveCount
        );

        if (hasMoved)
        {
            return;
        }

        int remainingStep = _maxInitialSteps - 1;
        while (remainingStep > 0)
        {
            stepPositionMask =
                (stepPositionMask & BitboardConstants.TopEdgeExcludeMask) << 10 & empty;
            if (stepPositionMask == 0)
            {
                break;
            }

            dest += 10;
            GenerateRegularStepPromotionMove(
                piece,
                from: position,
                to: dest,
                promotionEdgeMask: promotionEdgeMask,
                destBit: stepPositionMask,
                moves,
                ref moveCount
            );

            remainingStep--;
        }
    }

    private void GenerateRegularBlackPawnMoves(
        byte position,
        BitPiece piece,
        UInt128 positionBit,
        UInt128 empty,
        UInt128 enemyPieces,
        UInt128 promotionEdgeMask,
        bool hasMoved,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 captureRightMask =
            (positionBit & BitboardConstants.BottomRightEdgeExcludeMask) >> 9 & enemyPieces;
        if (captureRightMask != 0)
        {
            GenerateRegularCapturePromotionMove(
                piece,
                from: position,
                to: (byte)(position - 9),
                captureMask: captureRightMask,
                promotionEdgeMask: promotionEdgeMask,
                destBit: captureRightMask,
                moves,
                ref moveCount
            );
        }

        UInt128 captureLeftMask =
            (positionBit & BitboardConstants.BottomLeftEdgeExcludeMask) >> 11 & enemyPieces;
        if (captureLeftMask != 0)
        {
            GenerateRegularCapturePromotionMove(
                piece,
                from: position,
                to: (byte)(position - 11),
                captureMask: captureLeftMask,
                promotionEdgeMask: promotionEdgeMask,
                destBit: captureLeftMask,
                moves,
                ref moveCount
            );
        }

        UInt128 stepPositionMask =
            (positionBit & BitboardConstants.BottomEdgeExcludeMask) >> 10 & empty;
        if (stepPositionMask == 0)
        {
            return;
        }

        byte dest = (byte)(position - 10);
        GenerateRegularStepPromotionMove(
            piece,
            from: position,
            to: dest,
            promotionEdgeMask: promotionEdgeMask,
            destBit: stepPositionMask,
            moves,
            ref moveCount
        );

        if (hasMoved)
        {
            return;
        }

        int remainingStep = _maxInitialSteps - 1;
        while (remainingStep > 0)
        {
            stepPositionMask =
                (stepPositionMask & BitboardConstants.BottomEdgeExcludeMask) >> 10 & empty;
            if (stepPositionMask == 0)
            {
                break;
            }

            dest -= 10;
            GenerateRegularStepPromotionMove(
                piece,
                from: position,
                to: dest,
                promotionEdgeMask: promotionEdgeMask,
                destBit: stepPositionMask,
                moves,
                ref moveCount
            );

            remainingStep--;
        }
    }

    private void GenerateEnPassantMoves(
        BitPiece piece,
        byte position,
        UInt128 positionBit,
        BitBoard board,
        UInt128 enemyPieces,
        UInt128 promotionEdgeMask,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        if (board.EnPassantSquaresMask == 0)
        {
            return;
        }

        bool isWhite = piece.Color is BitPieceColor.White;
        (MagicPieceTable magicTable, byte enPassantSquare, int stepOffset)? enPassantResult;

        if (isWhite)
        {
            enPassantResult = GetWhiteEnPassant(board, positionBit, position);
        }
        else
        {
            enPassantResult = GetBlackEnPassant(board, positionBit, position);
        }
        if (enPassantResult is null)
        {
            return;
        }

        (MagicPieceTable magicTable, byte enPassantSquare, int stepOffset) = enPassantResult.Value;

        BitMove move = new()
        {
            From = position,
            To = enPassantSquare,
            Piece = piece,

            CapturesMask = UInt128.One << board.EnPassantPawnSquare,
            SpecialMoveType = SpecialMoveType.EnPassant,
            ForcedMovePriority = ForcedMovePriority.EnPassant,
        };
        moves[moveCount++] = move;

        UInt128 longPassantCaptures = MagicLibrary.GetAttacks(
            magicTable,
            enPassantSquare,
            occupancy: enemyPieces
        );
        // make sure no friendly pieces block the path
        UInt128 reachableLongPassantSquares = MagicLibrary.GetAttacks(
            magicTable,
            enPassantSquare,
            occupancy: board.Occupancy
        );
        longPassantCaptures &= enemyPieces;

        while (longPassantCaptures != 0)
        {
            // scan forward / backwards to make sure captures are right
            byte captureSquare = isWhite
                ? (byte)BitboardHelpers.BitScanForward(ref longPassantCaptures)
                : (byte)BitboardHelpers.BitScanBackward(ref longPassantCaptures);

            if ((reachableLongPassantSquares & (UInt128.One << captureSquare)) == 0)
            {
                break;
            }

            byte toSquare = (byte)(captureSquare + stepOffset);

            move.To = toSquare;
            move.CapturesMask |= UInt128.One << captureSquare;
            if ((UInt128.One << toSquare & promotionEdgeMask) != 0)
            {
                foreach (PieceType promoteTo in _promoteTo)
                {
                    move.PromotesTo = promoteTo;
                    moves[moveCount++] = move;
                }
            }
            else
            {
                moves[moveCount++] = move;
            }
        }
    }

    private static (
        MagicPieceTable enPassantTable,
        byte enPassantSquare,
        int stepOffset
    )? GetWhiteEnPassant(BitBoard board, UInt128 positionBit, byte position)
    {
        UInt128 rightEnPassant = board.EnPassantSquaresMask & positionBit << 11;
        if (rightEnPassant != 0 && (positionBit & BitboardConstants.RightEdgeMask) == 0)
        {
            return (
                enPassantTable: MagicLibrary.WhiteRightEnPassantTable,
                enPassantSquare: (byte)(position + 11),
                stepOffset: 10
            );
        }

        UInt128 leftEnPassant = board.EnPassantSquaresMask & positionBit << 9;
        if (leftEnPassant != 0 && (positionBit & BitboardConstants.LeftEdgeMask) == 0)
        {
            return (
                enPassantTable: MagicLibrary.WhiteLeftEnPassantTable,
                enPassantSquare: (byte)(position + 9),
                stepOffset: 10
            );
        }

        return null;
    }

    private static (
        MagicPieceTable enPassantTable,
        byte enPassantSquare,
        int stepOffset
    )? GetBlackEnPassant(BitBoard board, UInt128 positionBit, byte position)
    {
        UInt128 rightEnPassant = board.EnPassantSquaresMask & positionBit >> 9;
        if (rightEnPassant != 0 && (positionBit & BitboardConstants.RightEdgeMask) == 0)
        {
            return (
                enPassantTable: MagicLibrary.BlackRightEnPassantTable,
                enPassantSquare: (byte)(position - 9),
                stepOffset: -10
            );
        }

        UInt128 leftEnPassant = board.EnPassantSquaresMask & positionBit >> 11;
        if (leftEnPassant != 0 && (positionBit & BitboardConstants.LeftEdgeMask) == 0)
        {
            return (
                enPassantTable: MagicLibrary.BlackLeftEnPassantTable,
                enPassantSquare: (byte)(position - 11),
                stepOffset: -10
            );
        }

        return null;
    }

    private void GenerateRegularStepPromotionMove(
        BitPiece piece,
        byte from,
        byte to,
        UInt128 promotionEdgeMask,
        UInt128 destBit,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        GenerateRegularCapturePromotionMove(
            piece,
            from,
            to,
            captureMask: 0,
            promotionEdgeMask: promotionEdgeMask,
            destBit: destBit,
            moves,
            ref moveCount
        );
    }

    private void GenerateRegularCapturePromotionMove(
        BitPiece piece,
        byte from,
        byte to,
        UInt128 captureMask,
        UInt128 promotionEdgeMask,
        UInt128 destBit,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 stepPromotions = destBit & promotionEdgeMask;
        if (stepPromotions == 0)
        {
            moves[moveCount++] = new BitMove()
            {
                From = from,
                To = to,
                Piece = piece,
                CapturesMask = captureMask,
            };
        }
        else
        {
            foreach (PieceType promoteTo in _promoteTo)
            {
                moves[moveCount++] = new BitMove()
                {
                    From = from,
                    To = to,
                    Piece = piece,
                    PromotesTo = promoteTo,
                    CapturesMask = captureMask,
                };
            }
        }
    }

    private void GenerateSelfPromotionIfNeeded(
        BitPiece piece,
        byte position,
        UInt128 positionBit,
        UInt128 promotionEdgeMask,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 stepPromotions = positionBit & promotionEdgeMask;
        if (stepPromotions != 0)
        {
            foreach (PieceType promoteTo in _promoteTo)
            {
                moves[moveCount++] = new BitMove()
                {
                    From = position,
                    To = position,
                    Piece = piece,
                    PromotesTo = promoteTo,
                };
            }
        }
    }
}
