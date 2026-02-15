using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public interface IEvaluator
{
    int Evaluate(BitBoard board);
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
        ];

    public int Evaluate(BitBoard board)
    {
        bool isEndgame = IsEndgame(board);

        int whiteScore = 0;
        int blackScore = 0;

        foreach (var evaluator in _evaluators)
        {
            (int whiteResult, int blackResult) = evaluator.Evaluate(board, endgameFactor: 0);
            whiteScore += whiteResult;
            blackScore += blackResult;
        }

        return board.IsWhiteToMove ? whiteScore - blackScore : blackScore - whiteScore;
    }

    public static bool IsEndgame(BitBoard board) =>
        board.BitboardFor(PieceType.Queen, BitPieceColor.White) == 0
        && board.BitboardFor(PieceType.Queen, BitPieceColor.Black) == 0
        && board.WhiteMaterialCount + board.BlackMaterialCount <= 1800;
}
