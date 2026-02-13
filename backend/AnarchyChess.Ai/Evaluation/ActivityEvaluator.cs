using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class ActivityEvaluator
{
    // csharpier-ignore
    private static readonly int[] HorseyActivityTable =
    [
         -50, -45, -40, -35, -35, -35, -35, -40, -45, -50,
         -45, -30, -20, -10, -10, -10, -10, -20, -30, -45,
         -40, -20, -5,    0,   0,   0,   0,  -5, -20, -40,
         -35, -10,  0,   10,  15,  15,  10,   0, -10, -35,
         -35, -10,  0,   15,  20,  20,  15,   0, -10, -35,
         -35, -10,  0,   15,  20,  20,  15,   0, -10, -35,
         -35, -10,  0,   10,  15,  15,  10,   0, -10, -35,
         -40, -20, -5,    0,   0,   0,   0,  -5, -20, -40,
         -45, -30, -20, -10, -10, -10, -10, -20, -30, -45,
         -50, -45, -40, -35, -35, -35, -35, -40, -45, -50,
    ];

    // csharpier-ignore
    private static readonly int[] BishopActivityTable =
    [
         -20, -15, -10, -10, -10, -10, -10, -10, -15, -20 ,
         -15,  -5,   0,   0,   0,   0,   0,   0,  -5, -15 ,
         -10,   0,   5,   5,  10,  10,   5,   5,   0, -10 ,
         -10,   0,   5,  10,  15,  15,  10,   5,   0, -10 ,
         -10,   0,  10,  15,  20,  20,  15,  10,   0, -10 ,
         -10,   0,  10,  15,  20,  20,  15,  10,   0, -10 ,
         -10,   0,   5,  10,  15,  15,  10,   5,   0, -10 ,
         -10,   0,   5,   5,  10,  10,   5,   5,   0, -10 ,
         -15,  -5,   0,   0,   0,   0,   0,   0,  -5, -15 ,
         -20, -15, -10, -10, -10, -10, -10, -10, -15, -20 ,
    ];

    // csharpier-ignore
    private static readonly int[] CheckerActivityTable =
    [
        -50, -35, -30, -25, -25, -25, -25, -30, -35, -50,
        -50, -20, -10,  -5,  -5,  -5,  -5, -10, -20, -50,
        -45, -10,   0,   5,   5,   5,   5,   0, -10, -45,
        -40,  -5,   5,  10,  15,  15,  10,   5,  -5, -40,
        -40,  -5,   5,  15,  20,  20,  15,   5,  -5, -40,
        -40,  -5,   5,  15,  20,  20,  15,   5,  -5, -40,
        -40,  -5,   5,  10,  15,  15,  10,   5,  -5, -40,
        -45, -10,   0,   5,   5,   5,   5,   0, -10, -45,
        -50, -20, -10,  -5,  -5,  -5,  -5, -10, -20, -50,
        -50, -35, -30, -25, -25, -25, -25, -30, -35, -50,
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
