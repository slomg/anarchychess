using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

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
        bool isEndgame = IsEndgame(board);

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
        int kingSafteyScore = KingSafetyEvaluator.Evaluate(board, isEndgame: isEndgame);
        int aggressionScore = AggressionEvaluator.Evaluate(board);
        int pawnStructureScore = PawnStructureEvaluator.Evaluate(board);

        return materialScore
            + activityScore
            + mobilityScore
            + pawnSpaceScore
            + kingSafteyScore
            + aggressionScore
            + pawnSpaceScore
            + pawnStructureScore;
    }

    public static bool IsEndgame(BitBoard board) =>
        board.BitboardFor(PieceType.Queen, BitPieceColor.White) == 0
        && board.BitboardFor(PieceType.Queen, BitPieceColor.Black) == 0
        && board.WhiteMaterialCount + board.BlackMaterialCount <= 1800;
}
