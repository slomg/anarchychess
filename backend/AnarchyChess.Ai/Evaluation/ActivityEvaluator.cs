using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class ActivityEvaluator
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

    public static int Evaluate(BitBoard board, BitPieceColor ourColor, BitPieceColor enemyColor)
    {
        int activityScore = 0;

        activityScore += CalculateActivityScoreForPiece(
            ourBitboard: board.BitboardFor(PieceType.Horsey, ourColor)
                | board.BitboardFor(PieceType.Antiqueen, ourColor)
                | board.BitboardFor(PieceType.Knook, ourColor),
            enemyBitboard: board.BitboardFor(PieceType.Horsey, enemyColor)
                | board.BitboardFor(PieceType.Antiqueen, enemyColor)
                | board.BitboardFor(PieceType.Knook, enemyColor),
            activityTable: HorseyActivityTable
        );

        activityScore += CalculateActivityScoreForPiece(
            ourBitboard: board.BitboardFor(PieceType.Bishop, ourColor),
            enemyBitboard: board.BitboardFor(PieceType.Bishop, enemyColor),
            activityTable: BishopActivityTable
        );

        activityScore += CalculateActivityScoreForPiece(
            ourBitboard: board.BitboardFor(PieceType.Checker, ourColor),
            enemyBitboard: board.BitboardFor(PieceType.Checker, enemyColor),
            activityTable: CheckerActivityTable
        );

        return activityScore;
    }

    private static int CalculateActivityScoreForPiece(
        UInt128 ourBitboard,
        UInt128 enemyBitboard,
        int[] activityTable
    )
    {
        int activityScore = 0;
        while (ourBitboard != 0)
        {
            int position = BitboardHelpers.BitScanForward(ref ourBitboard);
            activityScore += activityTable[position];
        }

        while (enemyBitboard != 0)
        {
            int position = BitboardHelpers.BitScanForward(ref enemyBitboard);
            activityScore -= activityTable[position];
        }

        return activityScore;
    }
}
