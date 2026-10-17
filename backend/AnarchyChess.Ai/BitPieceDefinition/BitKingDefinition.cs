using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitKingDefinition : IBitPieceDefinition
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
        GenerateLaBastardaMoves(board, piece, position, moves, ref moveCount);
        GenerateCastleMovesForColor(board, piece, position, moves, ref moveCount);
    }

    private static void GenerateCastleMovesForColor(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        if (piece.Color is BitPieceColor.White)
        {
            GenerateCastleMoves(
                board,
                piece,
                position,
                BitboardConstants.WhiteKingsideCastle,
                moves,
                ref moveCount
            );
            GenerateCastleMoves(
                board,
                piece,
                position,
                BitboardConstants.WhiteQueensideCastle,
                moves,
                ref moveCount
            );
            GenerateCastleMoves(
                board,
                piece,
                position,
                BitboardConstants.WhiteVerticalCastle,
                moves,
                ref moveCount
            );
        }
        else
        {
            GenerateCastleMoves(
                board,
                piece,
                position,
                BitboardConstants.BlackKingsideCastle,
                moves,
                ref moveCount
            );
            GenerateCastleMoves(
                board,
                piece,
                position,
                BitboardConstants.BlackQueensideCastle,
                moves,
                ref moveCount
            );
            GenerateCastleMoves(
                board,
                piece,
                position,
                BitboardConstants.BlackVerticalCastle,
                moves,
                ref moveCount
            );
        }
    }

    private static void GenerateCastleMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        CastleInfo castleInfo,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        if (position != castleInfo.KingStart)
        {
            return;
        }

        UInt128 kingStartMask = UInt128.One << castleInfo.KingStart;
        UInt128 rookStartMask = UInt128.One << castleInfo.RookStart;

        if (
            (board.BitboardFor(piece.Type, piece.Color) & kingStartMask) == 0
            || (board.BitboardFor(PieceType.Rook, piece.Color) & rookStartMask) == 0
        )
        {
            return;
        }

        if (board.HasPieceMoved(kingStartMask) || board.HasPieceMoved(rookStartMask))
        {
            return;
        }

        if ((board.Occupancy & castleInfo.BetweenMask) != 0)
        {
            return;
        }

        UInt128 kingDestMask = UInt128.One << castleInfo.KingDest;
        UInt128 rookDestMask = UInt128.One << castleInfo.RookDest;

        UInt128 bishopBitboard = board.BitboardFor(PieceType.Bishop, piece.Color);

        UInt128 nonBishopBitboard = board.Occupancy & ~bishopBitboard;
        if ((nonBishopBitboard & kingDestMask) != 0 || (nonBishopBitboard & rookDestMask) != 0)
        {
            return;
        }

        UInt128 bishopCaptureMask = 0;
        if ((bishopBitboard & kingDestMask) != 0)
        {
            bishopCaptureMask |= UInt128.One << castleInfo.KingDest;
        }
        if ((bishopBitboard & rookDestMask) != 0)
        {
            bishopCaptureMask |= UInt128.One << castleInfo.RookDest;
        }

        BitMove move = new()
        {
            From = position,
            To = castleInfo.KingDest,
            Piece = piece,
            CapturesMask = bishopCaptureMask,
            SpecialMoveType = castleInfo.MoveType,
        };
        moves[moveCount++] = move;
    }

    private static void GenerateLaBastardaMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 friendlyPieces = board.BitboardForFriendOf(piece.Color);
        UInt128 attacks = PieceMasks.AdjacentMasks[position] & ~friendlyPieces;
        UInt128 positionBit = UInt128.One << position;
        UInt128 validQueens =
            board.BitboardFor(
                PieceType.Queen,
                piece.Color == BitPieceColor.White ? BitPieceColor.Black : BitPieceColor.White
            ) & ~board.StunnedPieces;
        var topLeftMask = (positionBit & BitboardConstants.TopLeftEdgeExcludeMask) << 9;
        var topMask = (positionBit & BitboardConstants.TopEdgeExcludeMask) << 10;
        var topRightMask = (positionBit & BitboardConstants.TopRightEdgeExcludeMask) << 11;
        var leftMask = (positionBit & BitboardConstants.LeftEdgeExcludeMask) >> 1;
        var rightMask = (positionBit & BitboardConstants.RightEdgeExcludeMask) << 1;
        var bottomLeftMask = (positionBit & BitboardConstants.BottomLeftEdgeExcludeMask) >> 11;
        var bottomMask = (positionBit & BitboardConstants.BottomEdgeExcludeMask) >> 10;
        var bottomRightMask = (positionBit & BitboardConstants.BottomRightEdgeExcludeMask) >> 9;
        if ((topLeftMask & attacks) != 0)
        {
            if (
                (
                    validQueens
                    & (topRightMask | rightMask | bottomLeftMask | bottomMask | bottomRightMask)
                ) != 0
            )
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 9),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 9),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((topRightMask & attacks) != 0)
        {
            if (
                (
                    validQueens
                    & (topLeftMask | leftMask | bottomLeftMask | bottomMask | bottomRightMask)
                ) != 0
            )
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 11),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 11),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((bottomLeftMask & attacks) != 0)
        {
            if (
                (validQueens & (bottomRightMask | rightMask | topLeftMask | topMask | topRightMask))
                != 0
            )
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 11),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 11),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((bottomRightMask & attacks) != 0)
        {
            if (
                (validQueens & (bottomLeftMask | leftMask | topLeftMask | topMask | topRightMask))
                != 0
            )
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 9),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 9),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((topMask & attacks) != 0)
        {
            if ((validQueens & (bottomLeftMask | bottomMask | bottomRightMask)) != 0)
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 10),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 10),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((leftMask & attacks) != 0)
        {
            if ((validQueens & (topRightMask | rightMask | bottomRightMask)) != 0)
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 1),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 1),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((rightMask & attacks) != 0)
        {
            if ((validQueens & (topLeftMask | leftMask | bottomLeftMask)) != 0)
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 1),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position + 1),
                    moves,
                    ref moveCount
                );
            }
        }
        if ((bottomMask & attacks) != 0)
        {
            if ((validQueens & (topLeftMask | topMask | topRightMask)) != 0)
            {
                GenerateLaBastardaMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 10),
                    moves,
                    ref moveCount
                );
            }
            else
            {
                GenerateRegularKingMove(
                    board,
                    piece,
                    position,
                    (byte)(position - 10),
                    moves,
                    ref moveCount
                );
            }
        }
    }

    private static void GenerateLaBastardaMove(
        BitBoard board,
        BitPiece piece,
        byte from,
        byte to,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        moves[moveCount++] = new BitMove()
        {
            From = from,
            To = to,
            Piece = piece,
            CapturesMask = (UInt128.One << to) & board.BitboardForEnemyOf(piece.Color),
            SpecialMoveType = SpecialMoveType.LaBastarda,
        };
    }

    private static void GenerateRegularKingMove(
        BitBoard board,
        BitPiece piece,
        byte from,
        byte to,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        moves[moveCount++] = new BitMove()
        {
            From = from,
            To = to,
            Piece = piece,
            CapturesMask = (UInt128.One << to) & board.BitboardForEnemyOf(piece.Color),
        };
    }
}
