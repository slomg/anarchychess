using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public interface IEvaluator
{
    int Evaluate(BitBoard board);
    bool TryEvaluateTermination(BitBoard board, int depth, out int eval);
}

public sealed class Evaluator(IEnumerable<IEvaluatorFunction>? evaluators = null) : IEvaluator
{
    private readonly IEvaluatorFunction[] _evaluators = evaluators is not null
        ? [.. evaluators]
        :
        [
            new ActivityEvaluator(),
            new AggressionEvaluator(),
            new KingSafetyEvaluator(),
            new MaterialEvaluator(),
            new MobilityEvaluator(),
            new PawnSpaceEvaluator(),
            new PawnStructureEvaluator(),
            new KingEndgameActivityEvaluator(),
        ];

    public int Evaluate(BitBoard board)
    {
        float endgameFactor = EndgameFactorCalculator.EndgameFactor(board);

        int whiteScore = 0;
        int blackScore = 0;

        foreach (var evaluator in _evaluators)
        {
            (int whiteResult, int blackResult) = evaluator.Evaluate(board, endgameFactor);
            whiteScore += whiteResult;
            blackScore += blackResult;
        }

        return board.IsWhiteToMove ? whiteScore - blackScore : blackScore - whiteScore;
    }

    public bool TryEvaluateTermination(BitBoard board, int depth, out int terminationEval)
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
