using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.Evaluation;

public interface IEvaluator
{
    int Evaluate(BitBoard board);
}

public sealed class Evaluator : IEvaluator
{
    public int Evaluate(BitBoard board)
    {
        BitPieceColor ourColor = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;
        BitPieceColor enemyColor = board.IsWhiteToMove ? BitPieceColor.Black : BitPieceColor.White;

        int materialScore = MaterialEvaluator.Evaluate(board, ourColor: ourColor);
        int activityScore = ActivityEvaluator.Evaluate(
            board,
            ourColor: ourColor,
            enemyColor: enemyColor
        );
        int mobilityScore = ActivityEvaluator.Evaluate(
            board,
            ourColor: ourColor,
            enemyColor: enemyColor
        );
        int pawnSpaceScore = PawnSpaceEvaluator.Evaluate(board);
        int kingSafteyScore = KingSafetyEvaluator.Evaluate(board);

        return materialScore + activityScore + mobilityScore + pawnSpaceScore + kingSafteyScore;
    }

    private static bool IsEndgame(BitBoard board) =>
        board.WhiteMaterialCount + board.BlackMaterialCount <= 2400;
}
