using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class AggressionEvaluator
{
    public const int MaxDistanceBonus = 20;

    public static EvaluationResult Evaluate(BitBoard board)
    {
        int whiteScore = 0;
        int blackScore = 0;

        UInt128 whiteKingBitboard = board.BitboardFor(PieceType.King, BitPieceColor.White);
        if (whiteKingBitboard != 0)
        {
            byte whiteKingSquare = BitboardHelpers.BitScanForward(ref whiteKingBitboard);
            blackScore += EvaluatePieceAggression(board, BitPieceColor.Black, whiteKingSquare);
        }

        UInt128 blackKingBitboard = board.BitboardFor(PieceType.King, BitPieceColor.Black);
        if (blackKingBitboard != 0)
        {
            byte blackKingSquare = BitboardHelpers.BitScanForward(ref blackKingBitboard);
            whiteScore += EvaluatePieceAggression(board, BitPieceColor.White, blackKingSquare);
        }

        return new() { WhiteScore = whiteScore, BlackScore = blackScore };
    }

    private static int EvaluatePieceAggression(
        BitBoard board,
        BitPieceColor color,
        byte targetKingSquare
    )
    {
        int score = 0;

        UInt128 pieces =
            board.BitboardFor(PieceType.Horsey, color)
            | board.BitboardFor(PieceType.Bishop, color)
            | board.BitboardFor(PieceType.Checker, color)
            | board.BitboardFor(PieceType.Rook, color)
            | board.BitboardFor(PieceType.Queen, color)
            | board.BitboardFor(PieceType.Knook, color)
            | board.BitboardFor(PieceType.Antiqueen, color);

        while (pieces != 0)
        {
            byte square = BitboardHelpers.BitScanForward(ref pieces);
            int distance = BitboardConstants.BoardDistance[square, targetKingSquare];
            score += Math.Max(0, MaxDistanceBonus - distance);
        }

        return score;
    }
}
