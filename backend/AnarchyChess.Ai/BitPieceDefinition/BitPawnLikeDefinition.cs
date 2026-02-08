using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitPawnLikeDefinition(PieceType[] promotesTo, int maxInitialSteps)
    : IBitPieceDefinition
{
    private readonly PieceType[] _promoteTo = promotesTo;
    private readonly int _maxInitialSteps = maxInitialSteps;

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
            promotionEdgeMask: promotionEdgeMask,
            moves,
            ref moveCount
        );

        BitboardHelpers.CreateMoveFromQuiets(position, pieceType, steps, moves, ref moveCount);
        BitboardHelpers.CreateMoveFromCaptures(position, pieceType, captures, moves, ref moveCount);
    }

    private void MaskWhitePawnMoves(
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

        int remainingStep = _maxInitialSteps - 1;
        while (remainingStep > 0)
        {
            stepPositionBit =
                (stepPositionBit & BitboardConstants.TopEdgeExcludeMask) << 10 & empty;
            steps |= stepPositionBit;
            remainingStep--;
        }
    }

    private void MaskBlackPawnMoves(
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

        int remainingStep = _maxInitialSteps - 1;
        while (remainingStep > 0)
        {
            stepPositionBit =
                (stepPositionBit & BitboardConstants.TopEdgeExcludeMask) >> 10 & empty;
            steps |= stepPositionBit;
            remainingStep--;
        }
    }

    private void GenerateEnPassantMoves(
        PieceType pieceType,
        bool isWhite,
        byte position,
        UInt128 positionBit,
        BitBoard board,
        UInt128 enemyPieces,
        UInt128 promotionEdgeMask,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        if (board.EnPassantSquares == 0)
        {
            return;
        }

        MagicPieceTable magicTable;
        byte enPassantSquare;
        int stepOffset;

        if (isWhite)
        {
            (magicTable, enPassantSquare, stepOffset) = GetWhiteEnPassant(
                board,
                positionBit,
                position
            );
        }
        else
        {
            (magicTable, enPassantSquare, stepOffset) = GetBlackEnPassant(
                board,
                positionBit,
                position
            );
        }

        BitMove move = new()
        {
            From = position,
            To = enPassantSquare,
            Piece = pieceType,

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
                foreach (PieceType promotePiece in _promoteTo)
                {
                    move.PromotesTo = promotePiece;
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
    ) GetWhiteEnPassant(BitBoard board, UInt128 positionBit, byte position)
    {
        UInt128 rightEnPassant = board.EnPassantSquares & positionBit << 11;
        if (rightEnPassant != 0)
        {
            return (
                enPassantTable: MagicLibrary.WhiteRightEnPassantTable,
                enPassantSquare: (byte)(position + 11),
                stepOffset: 10
            );
        }
        else
        {
            return (
                enPassantTable: MagicLibrary.WhiteLeftEnPassantTable,
                enPassantSquare: (byte)(position + 9),
                stepOffset: 10
            );
        }
    }

    private static (
        MagicPieceTable enPassantTable,
        byte enPassantSquare,
        int stepOffset
    ) GetBlackEnPassant(BitBoard board, UInt128 positionBit, byte position)
    {
        UInt128 rightEnPassant = board.EnPassantSquares & positionBit >> 9;
        if (rightEnPassant != 0)
        {
            return (
                enPassantTable: MagicLibrary.BlackRightEnPassantTable,
                enPassantSquare: (byte)(position - 9),
                stepOffset: -10
            );
        }
        else
        {
            return (
                enPassantTable: MagicLibrary.BlackLeftEnPassantTable,
                enPassantSquare: (byte)(position - 11),
                stepOffset: -10
            );
        }
    }

    private void GenerateRegularPromotionMoves(
        PieceType pieceType,
        byte position,
        ref UInt128 steps,
        ref UInt128 captures,
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

            foreach (PieceType piece in _promoteTo)
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

            BitMove move = new()
            {
                From = position,
                To = toSquare,
                Piece = pieceType,
                CapturesMask = UInt128.One << toSquare,
            };
            foreach (PieceType piece in _promoteTo)
            {
                move.PromotesTo = piece;
                moves[moveCount++] = move;
            }
        }
    }
}
