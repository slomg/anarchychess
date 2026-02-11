using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IAiEngine
{
    BitMove? FindBestMove(BitBoard board, int depth);
}

public class AiEngine(IBitMovesGenerator? moveGenerator = null, IEvaluator? evaluator = null)
    : IAiEngine
{
    private const int MaxMoves = 256;
    private readonly int PieceCount = Enum.GetValues<PieceType>().Length;

    private readonly IBitMovesGenerator _moveGenerator = moveGenerator ?? new BitMovesGenerator();
    private readonly IEvaluator _evaluator = evaluator ?? new Evaluator();

    public BitMove? FindBestMove(BitBoard board, int depth)
    {
        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        int moveCount = 0;
        Span<int> moveCountByPlace = stackalloc int[PieceCount];

        _moveGenerator.Generate(board, moves, ref moveCount, moveCountByPlace);
        OrderMove(board, moves, moveCount);

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
                beta: float.PositiveInfinity,
                prevMoveCountByPiece: moveCountByPlace
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

    private float Negamax(
        BitBoard board,
        int depth,
        float alpha,
        float beta,
        Span<int> prevMoveCountByPiece
    )
    {
        if (depth <= 0)
        {
            return _evaluator.EvaluateBoard(board, moveCountByPiece: prevMoveCountByPiece);
        }

        float best = float.NegativeInfinity;

        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        int moveCount = 0;
        Span<int> moveCountByPiece = stackalloc int[PieceCount];

        _moveGenerator.Generate(board, moves, ref moveCount, moveCountByPiece);
        OrderMove(board, moves, moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            MoveUndoState undo = board.MakeMove(move);

            float score = -Negamax(
                board,
                depth - 1,
                alpha: -beta,
                beta: -alpha,
                prevMoveCountByPiece: moveCountByPiece
            );

            board.UndoMove(undo);

            best = Math.Max(best, score);
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                break;
            }
        }

        return best;
    }

    private static void OrderMove(BitBoard board, Span<BitMove> moves, int moveCount)
    {
        int write = 0;
        for (int i = 0; i < moveCount; i++)
        {
            if (ScoreMove(moves[i], board) > 0)
            {
                if (i != write)
                {
                    (moves[write], moves[i]) = (moves[i], moves[write]);
                }
                write++;
            }
        }
    }

    private static int ScoreMove(BitMove move, BitBoard board)
    {
        if (move.CapturesMask != 0)
        {
            UInt128 captureMask = move.CapturesMask;
            int score = 0;
            while (captureMask != 0)
            {
                byte captureSquare = (byte)BitboardHelpers.BitScanForward(ref captureMask);
                if (board.TryGetPieceAt(captureSquare, out var capturePiece))
                {
                    score += (int)Evaluator.GetPieceValue(capturePiece.Value.Type);
                }
            }
            return 10_000 + score;
        }

        if (move.PromotesTo is not null)
        {
            return 9_000 + (int)Evaluator.GetPieceValue(move.PromotesTo.Value);
        }

        return 0;
    }
}
