using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai;

public interface IAiEngine
{
    BitMove? FindBestMove(BitBoard board, int depth);
}

public class AiEngine(IBitMovesGenerator? moveGenerator = null, IEvaluator? evaluator = null)
    : IAiEngine
{
    private const int MaxMoves = 256;

    private readonly IBitMovesGenerator _moveGenerator = moveGenerator ?? new BitMovesGenerator();
    private readonly IEvaluator _evaluator = evaluator ?? new Evaluator();

    public BitMove? FindBestMove(BitBoard board, int depth)
    {
        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        int moveCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount);

        BitMove? bestMove = null;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            MoveUndoState undo = board.MakeMove(move);

            float score = -Negamax(
                board,
                depth - 1,
                alpha: float.NegativeInfinity,
                beta: float.PositiveInfinity
            );

            board.UndoMove(undo);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private float Negamax(BitBoard board, int depth, float alpha, float beta)
    {
        if (depth <= 0)
        {
            return _evaluator.EvaluateBoard(board);
        }

        float best = float.NegativeInfinity;

        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        int moveCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            UInt128 whiteBefore = board.WhitePieces;

            BitMove move = moves[i];
            MoveUndoState undo = board.MakeMove(move);

            float score = -Negamax(board, depth - 1, alpha: -beta, beta: -alpha);

            board.UndoMove(undo);

            UInt128 whiteAfter = board.WhitePieces;

            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                break;
            }
        }

        return best;
    }
}
