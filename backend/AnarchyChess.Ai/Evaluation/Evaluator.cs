using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public struct EvaluationResult
{
    public int WhiteScore;
    public int BlackScore;
}

public static class Evaluator
{
    public static int Evaluate(BitBoard board)
    {
        float endgameFactor = EndgameFactorCalculator.EndgameFactor(board);

        EvaluationResult activity = ActivityEvaluator.Evaluate(board);
        EvaluationResult aggression = AggressionEvaluator.Evaluate(board);
        EvaluationResult kingSafety = KingSafetyEvaluator.Evaluate(board, endgameFactor);
        EvaluationResult material = MaterialEvaluator.Evaluate(board);
        EvaluationResult mobility = MobilityEvaluator.Evaluate(board);
        EvaluationResult pawnSpace = PawnSpaceEvaluator.Evaluate(board);
        EvaluationResult pawnStructure = PawnStructureEvaluator.Evaluate(board);
        EvaluationResult kingEndgameActivity = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor
        );

        int whiteScore =
            activity.WhiteScore
            + aggression.WhiteScore
            + kingSafety.WhiteScore
            + material.WhiteScore
            + mobility.WhiteScore
            + pawnSpace.WhiteScore
            + pawnStructure.WhiteScore
            + kingEndgameActivity.WhiteScore;
        int blackScore =
            activity.BlackScore
            + aggression.BlackScore
            + kingSafety.BlackScore
            + material.BlackScore
            + mobility.BlackScore
            + pawnSpace.BlackScore
            + pawnStructure.BlackScore
            + kingEndgameActivity.BlackScore;

        return board.IsWhiteToMove ? whiteScore - blackScore : blackScore - whiteScore;
    }

    public static bool TryEvaluateTermination(BitBoard board, int depth, out int terminationEval)
    {
        UInt128 whiteKings = board.BitboardFor(PieceType.King, BitPieceColor.White);
        UInt128 blackKings = board.BitboardFor(PieceType.King, BitPieceColor.Black);

        if (whiteKings == 0 && blackKings == 0)
        {
            terminationEval = 0;
            return true;
        }

        if (whiteKings == 0)
        {
            terminationEval = board.IsWhiteToMove ? -100_000 - depth : 100_000 + depth;
            return true;
        }
        else if (blackKings == 0)
        {
            terminationEval = board.IsWhiteToMove ? 100_000 + depth : -100_000 - depth;
            return true;
        }

        while (whiteKings != 0)
        {
            byte kingPos = BitboardHelpers.BitScanForward(ref whiteKings);
            if ((PieceMasks.AdjacentMasks[kingPos] & blackKings) != 0)
            {
                terminationEval = 0;
                return true;
            }
        }

        terminationEval = 0;
        return false;
    }
}
