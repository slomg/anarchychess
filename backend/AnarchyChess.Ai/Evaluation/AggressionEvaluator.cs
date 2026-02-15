using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class AggressionEvaluator : IEvaluatorFunction
{
    public const int MaxDistanceBonus = 20;

    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        int whiteScore = 0;
        int blackScore = 0;

        UInt128 whiteKingBitboard = board.BitboardFor(PieceType.King, BitPieceColor.White);
        if (whiteKingBitboard != 0)
        {
            byte whiteKingSquare = (byte)BitboardHelpers.BitScanForward(ref whiteKingBitboard);
            blackScore += EvaluatePieceAggression(board, BitPieceColor.Black, whiteKingSquare);
        }

        UInt128 blackKingBitboard = board.BitboardFor(PieceType.King, BitPieceColor.Black);
        if (blackKingBitboard != 0)
        {
            byte blackKingSquare = (byte)BitboardHelpers.BitScanForward(ref blackKingBitboard);
            whiteScore += EvaluatePieceAggression(board, BitPieceColor.White, blackKingSquare);
        }

        return (WhiteScore: whiteScore, BlackScore: blackScore);
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
            byte square = (byte)BitboardHelpers.BitScanForward(ref pieces);
            int distance = ChebyshevDistance(square, targetKingSquare);
            score += Math.Max(0, MaxDistanceBonus - distance);
        }

        return score;
    }

    private static int ChebyshevDistance(int sq1, int sq2)
    {
        int file1 = sq1 % 10;
        int rank1 = sq1 / 10;
        int file2 = sq2 % 10;
        int rank2 = sq2 / 10;
        return Math.Max(Math.Abs(file1 - file2), Math.Abs(rank1 - rank2));
    }
}
