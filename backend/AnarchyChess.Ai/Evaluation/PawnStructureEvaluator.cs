using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class PawnStructureEvaluator : IEvaluatorFunction
{
    public const int DoubledPenalty = 12;
    public const int IsolatedPenalty = 18;
    public const int BackwardsPenalty = 10;
    public const int PassedBonus = 35;

    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        int whiteScore = 0;
        int blackScore = 0;

        UInt128 whitePawns =
            board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
            | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White);
        UInt128 blackPawns =
            board.BitboardFor(PieceType.Pawn, BitPieceColor.Black)
            | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black);

        for (int file = 0; file < 10; file++)
        {
            whiteScore -= CountDoubled(whitePawns, file) * DoubledPenalty;
            blackScore -= CountDoubled(blackPawns, file) * DoubledPenalty;
        }

        whiteScore -= CountIsolated(whitePawns) * IsolatedPenalty;
        blackScore -= CountIsolated(blackPawns) * IsolatedPenalty;

        whiteScore -= CountWhiteBackwards(whitePawns, enemyPawns: blackPawns) * BackwardsPenalty;
        blackScore -= CountBlackBackwards(blackPawns, enemyPawns: whitePawns) * BackwardsPenalty;

        whiteScore += CountWhitePassed(whitePawns, enemyPawns: blackPawns) * PassedBonus;
        blackScore += CountBlackPassed(blackPawns, enemyPawns: whitePawns) * PassedBonus;

        return (WhiteScore: whiteScore, BlackScore: blackScore);
    }

    private static int CountDoubled(UInt128 pawns, int file) =>
        Math.Max(0, BitboardHelpers.CountBits(pawns & BitboardConstants.FileMasks[file]) - 1);

    private static int CountIsolated(UInt128 pawns)
    {
        UInt128 rightExclude = pawns & BitboardConstants.RightEdgeExcludeMask;
        UInt128 rightNeighbors = rightExclude << 1 | rightExclude << 11 | rightExclude >> 9;

        UInt128 leftExclude = pawns & BitboardConstants.LeftEdgeExcludeMask;
        UInt128 leftNeighbors = leftExclude >> 1 | leftExclude << 9 | leftExclude >> 11;

        UInt128 nonIsolated = pawns & (leftNeighbors | rightNeighbors);
        UInt128 isolated = pawns & ~nonIsolated;

        return BitboardHelpers.CountBits(isolated);
    }

    private static int CountWhiteBackwards(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 defendedPawns =
            ((pawns & BitboardConstants.RightEdgeExcludeMask) << 9)
            | ((pawns & BitboardConstants.LeftEdgeExcludeMask) << 11);
        UInt128 undefendedPawns = pawns & ~defendedPawns;

        UInt128 blockedPawns = enemyPawns >> 10;
        undefendedPawns &= ~blockedPawns;

        UInt128 adjacentFiles =
            ((pawns & BitboardConstants.LeftEdgeExcludeMask) << 1)
            | ((pawns & BitboardConstants.RightEdgeExcludeMask) >> 1);
        undefendedPawns &= ~adjacentFiles;

        UInt128 enemyAttacks =
            ((enemyPawns & BitboardConstants.LeftEdgeExcludeMask) >> 9)
            | ((enemyPawns & BitboardConstants.RightEdgeExcludeMask) >> 11);

        UInt128 front = undefendedPawns << 10;
        UInt128 backwardPawns = front & enemyAttacks;

        return BitboardHelpers.CountBits(backwardPawns);
    }

    private static int CountBlackBackwards(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 defendedPawns =
            ((pawns & BitboardConstants.RightEdgeExcludeMask) >> 11)
            | ((pawns & BitboardConstants.LeftEdgeExcludeMask) >> 9);
        UInt128 undefendedPawns = pawns & ~defendedPawns;

        UInt128 blockedPawns = enemyPawns << 10;
        undefendedPawns &= ~blockedPawns;

        UInt128 adjacentFiles =
            ((pawns & BitboardConstants.LeftEdgeExcludeMask) << 1)
            | ((pawns & BitboardConstants.RightEdgeExcludeMask) >> 1);
        undefendedPawns &= ~adjacentFiles;

        UInt128 enemyAttacks =
            ((enemyPawns & BitboardConstants.LeftEdgeExcludeMask) << 11)
            | ((enemyPawns & BitboardConstants.RightEdgeExcludeMask) << 9);

        UInt128 front = undefendedPawns >> 10;
        UInt128 backwardPawns = front & enemyAttacks;
        return BitboardHelpers.CountBits(backwardPawns);
    }

    private static int CountWhitePassed(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 mask = enemyPawns;
        for (int file = 0; file < 10; file++)
        {
            mask |= mask >> 10;
        }
        mask &= (UInt128.One << 100) - 1;

        UInt128 leftMask = (mask & BitboardConstants.LeftEdgeExcludeMask) >> 1;
        UInt128 rightMask = (mask & BitboardConstants.RightEdgeExcludeMask) << 1;

        UInt128 blocking = mask | leftMask | rightMask;
        return BitboardHelpers.CountBits(pawns & ~blocking);
    }

    private static int CountBlackPassed(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 mask = enemyPawns;
        for (int file = 0; file < 10; file++)
        {
            mask |= mask << 10;
        }
        mask &= (UInt128.One << 100) - 1;

        UInt128 leftMask = (mask & BitboardConstants.LeftEdgeExcludeMask) >> 1;
        UInt128 rightMask = (mask & BitboardConstants.RightEdgeExcludeMask) << 1;

        UInt128 blocking = mask | leftMask | rightMask;
        return BitboardHelpers.CountBits(pawns & ~blocking);
    }
}
