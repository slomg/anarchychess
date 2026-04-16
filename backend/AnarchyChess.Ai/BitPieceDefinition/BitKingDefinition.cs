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
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 friendlyPieces = board.BitboardForFriendOf(piece.Color);

        UInt128 attacks = PieceMasks.AdjacentMasks[position];
        attacks &= ~friendlyPieces;

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            piece,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );
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
}
