namespace AnarchyChess.Ai.Evaluation;

public interface IEvaluatorFunction
{
    (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor);
}
