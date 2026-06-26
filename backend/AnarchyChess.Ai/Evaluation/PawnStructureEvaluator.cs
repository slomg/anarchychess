using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class PawnStructureEvaluator
{
    public const int DoubledPenalty = 12;
    public const int IsolatedPenalty = 25;
    public const int BackwardsPenalty = 20;
    public const int PassedBonus = 35;

    public static EvaluationResult Evaluate(BitBoard board)
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

        return new() { WhiteScore = whiteScore, BlackScore = blackScore };
    }

    private static int CountDoubled(UInt128 pawns, int file) =>
        Math.Max(0, BitboardHelpers.CountBits(pawns & BitboardConstants.FileMasks[file]) - 1);

    private static int CountIsolated(UInt128 pawns)
    {
        UInt128 rightNeighbors =
            BitboardHelpers.ShiftRight(pawns)
            | BitboardHelpers.ShiftUpRight(pawns)
            | BitboardHelpers.ShiftDownRight(pawns);

        UInt128 leftNeighbors =
            BitboardHelpers.ShiftLeft(pawns)
            | BitboardHelpers.ShiftUpLeft(pawns)
            | BitboardHelpers.ShiftDownLeft(pawns);

        UInt128 nonIsolated = pawns & (leftNeighbors | rightNeighbors);
        UInt128 isolated = pawns & ~nonIsolated;

        return BitboardHelpers.CountBits(isolated);
    }

    private static int CountWhiteBackwards(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 defendedPawns =
            BitboardHelpers.ShiftUpLeft(pawns) | BitboardHelpers.ShiftUpRight(pawns);
        UInt128 undefendedPawns = pawns & ~defendedPawns;

        UInt128 blockedPawns = BitboardHelpers.ShiftDown(enemyPawns);
        undefendedPawns &= ~blockedPawns;

        UInt128 adjacentFiles =
            BitboardHelpers.ShiftLeft(pawns) | BitboardHelpers.ShiftRight(pawns);
        undefendedPawns &= ~adjacentFiles;

        UInt128 enemyAttacks =
            BitboardHelpers.ShiftDownRight(enemyPawns) | BitboardHelpers.ShiftDownLeft(enemyPawns);

        UInt128 front = BitboardHelpers.ShiftUp(undefendedPawns);
        UInt128 backwardPawns = front & enemyAttacks;

        return BitboardHelpers.CountBits(backwardPawns);
    }

    private static int CountBlackBackwards(UInt128 pawns, UInt128 enemyPawns)
    {
        UInt128 defendedPawns =
            BitboardHelpers.ShiftDownLeft(pawns) | BitboardHelpers.ShiftDownRight(pawns);
        UInt128 undefendedPawns = pawns & ~defendedPawns;

        UInt128 blockedPawns = BitboardHelpers.ShiftUp(enemyPawns);
        undefendedPawns &= ~blockedPawns;

        UInt128 adjacentFiles =
            BitboardHelpers.ShiftRight(pawns) | BitboardHelpers.ShiftLeft(pawns);
        undefendedPawns &= ~adjacentFiles;

        UInt128 enemyAttacks =
            BitboardHelpers.ShiftUpRight(enemyPawns) | BitboardHelpers.ShiftUpLeft(enemyPawns);

        UInt128 front = BitboardHelpers.ShiftDown(undefendedPawns);
        UInt128 backwardPawns = front & enemyAttacks;
        return BitboardHelpers.CountBits(backwardPawns);
    }

    private static int CountWhitePassed(UInt128 pawns, UInt128 enemyPawns) =>
        BitboardHelpers.CountBits(PawnStructureHelpers.GetWhitePassed(pawns, enemyPawns));

    private static int CountBlackPassed(UInt128 pawns, UInt128 enemyPawns) =>
        BitboardHelpers.CountBits(PawnStructureHelpers.GetBlackPassed(pawns, enemyPawns));
}
