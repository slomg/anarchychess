using System.Runtime.CompilerServices;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class KingEndgameActivityEvaluator : IEvaluatorFunction
{
    public const int CenterProximityBonus = 5;
    public const int EnemyPawnProximityBonus = 10;
    public const int OwnPassedPawnProximityBonus = 20;

    public const float EndgameFactorThreshold = 0.2f;

    private static readonly byte Center = new AlgebraicPoint("f5").AsIdx();

    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        if (endgameFactor < EndgameFactorThreshold)
        {
            return (0, 0);
        }

        UInt128 whitePawns =
            board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
            | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White);
        UInt128 blackPawns =
            board.BitboardFor(PieceType.Pawn, BitPieceColor.Black)
            | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black);

        UInt128 passedWhitePawns = PawnStructureHelpers.GetWhitePassed(
            whitePawns,
            enemyPawns: blackPawns
        );
        UInt128 passedBlackPawns = PawnStructureHelpers.GetBlackPassed(
            blackPawns,
            enemyPawns: whitePawns
        );

        UInt128 whiteKings = board.BitboardFor(PieceType.King, BitPieceColor.White);
        UInt128 blackKings = board.BitboardFor(PieceType.King, BitPieceColor.Black);

        int whiteScore = 0;
        int blackScore = 0;

        while (whiteKings != 0)
        {
            byte kingPosition = BitboardHelpers.BitScanForward(ref whiteKings);
            whiteScore += DistanceToPassedPawn(kingPosition, passedWhitePawns);
            whiteScore += DistanceToEnemyPawn(
                kingPosition,
                blackPawns,
                enemyPassedPawns: passedBlackPawns
            );
            whiteScore += DistanceToCenter(kingPosition);
        }

        while (blackKings != 0)
        {
            byte kingPosition = BitboardHelpers.BitScanForward(ref blackKings);
            blackScore += DistanceToPassedPawn(kingPosition, passedBlackPawns);
            blackScore += DistanceToEnemyPawn(
                kingPosition,
                whitePawns,
                enemyPassedPawns: passedWhitePawns
            );
            blackScore += DistanceToCenter(kingPosition);
        }

        return (
            WhiteScore: (int)(whiteScore * endgameFactor),
            BlackScore: (int)(blackScore * endgameFactor)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DistanceToPassedPawn(int kingPosition, UInt128 passedPawns)
    {
        int minDistance = int.MaxValue;

        while (passedPawns != 0)
        {
            byte pawnSquare = BitboardHelpers.BitScanForward(ref passedPawns);
            minDistance = Math.Min(
                minDistance,
                BitboardConstants.BoardDistance[kingPosition, pawnSquare]
            );
        }

        return minDistance == int.MaxValue ? 0 : OwnPassedPawnProximityBonus - minDistance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DistanceToEnemyPawn(
        int kingPosition,
        UInt128 enemyPawns,
        UInt128 enemyPassedPawns
    )
    {
        int minTotalDistance = int.MaxValue;
        int minDistancePassed = int.MaxValue;

        while (enemyPawns != 0)
        {
            byte pawnSquare = BitboardHelpers.BitScanForward(ref enemyPawns);
            int distance = BitboardConstants.BoardDistance[kingPosition, pawnSquare];
            minTotalDistance = Math.Min(minTotalDistance, distance);

            if ((enemyPassedPawns & (UInt128.One << pawnSquare)) != 0)
            {
                minDistancePassed = Math.Min(minDistancePassed, distance);
            }
        }

        int score = 0;
        if (minDistancePassed != int.MaxValue)
        {
            score += EnemyPawnProximityBonus - minDistancePassed;
        }
        else if (minTotalDistance != int.MaxValue)
        {
            score += EnemyPawnProximityBonus - minTotalDistance;
        }

        return score;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DistanceToCenter(int kingPosition) =>
        CenterProximityBonus - BitboardConstants.BoardDistance[kingPosition, Center];
}
