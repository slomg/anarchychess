using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class KingSafetyEvaluator
{
    public const int PawnProtectionValue = 2;
    public const int EdgeAmplifier = 2;

    public const int CenterStuckKingPenalty = 100;
    public const int SemiStuckKingPenalty = 20;

    public const float EndgameFactorThreshold = 0.8f;

    private static readonly UInt128 WhiteKingsideRookMask =
        UInt128.One << BitboardConstants.WhiteKingsideCastle.RookStart;
    private static readonly UInt128 WhiteQueensideRookMask =
        UInt128.One << BitboardConstants.WhiteQueensideCastle.RookStart;

    private static readonly UInt128 BlackKingsideRookMask =
        UInt128.One << BitboardConstants.BlackKingsideCastle.RookStart;
    private static readonly UInt128 BlackQueensideRookMask =
        UInt128.One << BitboardConstants.BlackQueensideCastle.RookStart;

    public static EvaluationResult Evaluate(BitBoard board, float endgameFactor)
    {
        if (endgameFactor >= EndgameFactorThreshold)
        {
            return new();
        }

        float kingSafetyWeight = 1f - endgameFactor;

        int whiteKingSafety = EvaluateKingSpace(
            board,
            kingBitboard: board.BitboardFor(PieceType.King, BitPieceColor.White),
            pawnBitboard: board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
                | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White),
            rookBitboard: board.BitboardFor(PieceType.Rook, BitPieceColor.White),
            kingsideRookMask: WhiteKingsideRookMask,
            queensideRookMask: WhiteQueensideRookMask
        );
        int whiteScore = (int)(whiteKingSafety * kingSafetyWeight);

        int blackKingSafety = EvaluateKingSpace(
            board,
            kingBitboard: board.BitboardFor(PieceType.King, BitPieceColor.Black),
            pawnBitboard: board.BitboardFor(PieceType.Pawn, BitPieceColor.Black)
                | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black),
            rookBitboard: board.BitboardFor(PieceType.Rook, BitPieceColor.Black),
            kingsideRookMask: BlackKingsideRookMask,
            queensideRookMask: BlackQueensideRookMask
        );
        int blackScore = (int)(blackKingSafety * kingSafetyWeight);

        return new() { WhiteScore = whiteScore, BlackScore = blackScore };
    }

    private static int EvaluateKingSpace(
        BitBoard board,
        UInt128 kingBitboard,
        UInt128 pawnBitboard,
        UInt128 rookBitboard,
        UInt128 kingsideRookMask,
        UInt128 queensideRookMask
    )
    {
        int score = 0;

        while (kingBitboard != 0)
        {
            byte square = BitboardHelpers.BitScanForward(ref kingBitboard);

            UInt128 kingAdjacent = PieceMasks.AdjacentMasks[square];
            int numOfPawnsAroundKing = BitboardHelpers.CountBits(kingAdjacent & pawnBitboard);

            int file = square % 10;
            bool isCentralFile = file >= 4 && file <= 5;

            int edgeAmplifier = isCentralFile ? 1 : EdgeAmplifier;
            score += numOfPawnsAroundKing * PawnProtectionValue * edgeAmplifier;

            bool hasMoved = board.HasPieceMoved(square);
            bool canCastleKingside =
                !hasMoved
                && (rookBitboard & kingsideRookMask) != 0
                && !board.HasPieceMoved(kingsideRookMask);
            bool canCastleQueenside =
                !hasMoved
                && (rookBitboard & queensideRookMask) != 0
                && !board.HasPieceMoved(queensideRookMask);
            if (isCentralFile && !canCastleKingside && !canCastleQueenside)
            {
                score -= CenterStuckKingPenalty;
            }
            else if (isCentralFile && (!canCastleKingside || !canCastleQueenside))
            {
                score -= SemiStuckKingPenalty;
            }
        }

        return score;
    }
}
