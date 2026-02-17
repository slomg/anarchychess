using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class ActivityEvaluator : IEvaluatorFunction
{
    // csharpier-ignore
    public static readonly int[] HorseyActivityTable =
    [
        -25, -22, -20, -17, -17, -17, -17, -20, -22, -25,
        -22, -15, -10,  -5,  -5,  -5,  -5, -10, -15, -22,
        -20, -10,  -2,   0,   0,   0,   0,  -2, -10, -20,
        -17,  -5,   0,   5,   7,   7,   5,   0,  -5, -17,
        -17,  -5,   0,   7,  10,  10,   7,   0,  -5, -17,
        -17,  -5,   0,   7,  10,  10,   7,   0,  -5, -17,
        -17,  -5,   0,   5,   7,   7,   5,   0,  -5, -17,
        -20, -10,  -2,   0,   0,   0,   0,  -2, -10, -20,
        -22, -15, -10,  -5,  -5,  -5,  -5, -10, -15, -22,
        -25, -22, -20, -17, -17, -17, -17, -20, -22, -25,
    ];

    // csharpier-ignore
    public static readonly int[] BishopActivityTable =
    [
        -10,  -8,  -5,  -5,  -5,  -5,  -5,  -5,  -8, -10,
        -8,   -2,   0,   0,   0,   0,   0,   0,  -2,  -8,
        -5,    0,   2,   2,   5,   5,   2,   2,   0,  -5,
        -5,    0,   2,   5,   7,   7,   5,   2,   0,  -5,
        -5,    0,   5,   7,  10,  10,   7,   5,   0,  -5,
        -5,    0,   5,   7,  10,  10,   7,   5,   0,  -5,
        -5,    0,   2,   5,   7,   7,   5,   2,   0,  -5,
        -5,    0,   2,   2,   5,   5,   2,   2,   0,  -5,
        -8,   -2,   0,   0,   0,   0,   0,   0,  -2,  -8,
        -10,  -8,  -5,  -5,  -5,  -5,  -5,  -5,  -8, -10,
    ];

    // csharpier-ignore
    public static readonly int[] CheckerActivityTable =
    [
        -25, -17, -15, -12, -12, -12, -12, -15, -17, -25,
        -25, -10,  -5,  -2,  -2,  -2,  -2,  -5, -10, -25,
        -22,  -5,   0,   2,   2,   2,   2,   0,  -5, -22,
        -20,  -2,   2,   5,   7,   7,   5,   2,  -2, -20,
        -20,  -2,   2,   7,  10,  10,   7,   2,  -2, -20,
        -20,  -2,   2,   7,  10,  10,   7,   2,  -2, -20,
        -20,  -2,   2,   5,   7,   7,   5,   2,  -2, -20,
        -22,  -5,   0,   2,   2,   2,   2,   0,  -5, -22,
        -25, -10,  -5,  -2,  -2,  -2,  -2,  -5, -10, -25,
        -25, -17, -15, -12, -12, -12, -12, -15, -17, -25,
    ];

    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        int whiteScore = 0;
        int blackScore = 0;

        whiteScore += CalculateActivityScoreForPiece(
            board.BitboardFor(PieceType.Horsey, BitPieceColor.White)
                | board.BitboardFor(PieceType.Antiqueen, BitPieceColor.White)
                | board.BitboardFor(PieceType.Knook, BitPieceColor.White),
            HorseyActivityTable
        );
        blackScore += CalculateActivityScoreForPiece(
            board.BitboardFor(PieceType.Horsey, BitPieceColor.Black)
                | board.BitboardFor(PieceType.Antiqueen, BitPieceColor.Black)
                | board.BitboardFor(PieceType.Knook, BitPieceColor.Black),
            HorseyActivityTable
        );

        whiteScore += CalculateActivityScoreForPiece(
            board.BitboardFor(PieceType.Bishop, BitPieceColor.White),
            BishopActivityTable
        );
        blackScore += CalculateActivityScoreForPiece(
            board.BitboardFor(PieceType.Bishop, BitPieceColor.Black),
            BishopActivityTable
        );

        whiteScore += CalculateActivityScoreForPiece(
            board.BitboardFor(PieceType.Checker, BitPieceColor.White),
            CheckerActivityTable
        );
        blackScore += CalculateActivityScoreForPiece(
            board.BitboardFor(PieceType.Checker, BitPieceColor.Black),
            CheckerActivityTable
        );

        return (WhiteScore: whiteScore, BlackScore: blackScore);
    }

    private static int CalculateActivityScoreForPiece(UInt128 bitboard, int[] activityTable)
    {
        int score = 0;
        while (bitboard != 0)
        {
            int position = BitboardHelpers.BitScanForward(ref bitboard);
            score += activityTable[position];
        }

        return score;
    }
}
