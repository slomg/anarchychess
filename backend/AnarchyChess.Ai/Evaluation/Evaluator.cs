namespace AnarchyChess.Ai.Evaluation;

public interface IEvaluator
{
    int Evaluate(BitBoard board);
}

public sealed class Evaluator(
    IEndgameFactorCalculator? endgameFactorCalculator = null,
    IEnumerable<IEvaluatorFunction>? evaluators = null
) : IEvaluator
{
    private readonly IEndgameFactorCalculator _endgameFactorCalculator =
        endgameFactorCalculator ?? new EndgameFactorCalculator();
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
        float endgameFactor = _endgameFactorCalculator.EndgameFactor(board);

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
}
