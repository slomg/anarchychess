using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class KingSafetyEvaluator : IEvaluatorFunction
{
    public const int PawnProtectionValue = 2;
    public const int EdgeAmplifier = 2;

    public const int CenterStuckKingPenalty = 50;
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

    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        if (endgameFactor > EndgameFactorThreshold)
        {
            return (0, 0);
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

        return (WhiteScore: whiteScore, BlackScore: blackScore);
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
            byte square = (byte)BitboardHelpers.BitScanForward(ref kingBitboard);

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
