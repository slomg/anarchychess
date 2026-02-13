using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IAiEngine
{
    BitMove? FindBestMove(BitBoard board, int depth);
}

public class AiEngine(IBitMoveGenerator? moveGenerator = null, IEvaluator? evaluator = null)
    : IAiEngine
{
    private const int ALPHA_START = -10_000_000;
    private const int BETA_START = 10_000_000;

    private const int MaxMoves = 256;
    private const int NullMoveReduction = 2;
    private readonly int PieceCount = Enum.GetValues<PieceType>().Length;

    private readonly IBitMoveGenerator _moveGenerator = moveGenerator ?? new BitMoveGenerator();
    private readonly IEvaluator _evaluator = evaluator ?? new Evaluator();

    private BitMove[,] _killerMoves = new BitMove[0, 0];

    public BitMove? FindBestMove(BitBoard board, int depth)
    {
        _killerMoves = new BitMove[depth + 1, 2];

        BitMove[] moves = new BitMove[MaxMoves];
        int moveCount = 0;

        _moveGenerator.Generate(board, moves, ref moveCount);
        OrderMoves(board, depth, moves, moveCount);

        if (moveCount == 0)
            return null;

        BitMove bestMove = moves[0];
        BitBoard boardCopy = new(board);
        boardCopy.MakeMove(bestMove);

        int alpha = -Negamax(boardCopy, depth - 1, alpha: ALPHA_START, beta: BETA_START);

        int[] scores = new int[moveCount];

        Parallel.For(
            1,
            moveCount,
            i =>
            {
                BitMove move = moves[i];
                BitBoard boardCopy = new(board);
                boardCopy.MakeMove(move);

                int score = -Negamax(boardCopy, depth - 1, alpha: -alpha - 1, beta: -alpha);
                if (score > alpha)
                {
                    score = -Negamax(boardCopy, depth - 1, alpha: ALPHA_START, beta: BETA_START);
                }

                scores[i] = score;
            }
        );

        for (int i = 1; i < moveCount; i++)
        {
            if (scores[i] > alpha)
            {
                alpha = scores[i];
                bestMove = moves[i];
            }
        }

        Console.WriteLine($"Eval: {alpha}");

        return bestMove;
    }

    private int Negamax(BitBoard board, int depth, int alpha, int beta)
    {
        if (
            depth <= 0
            || board.BitboardFor(PieceType.King, BitPieceColor.White) == 0
            || board.BitboardFor(PieceType.King, BitPieceColor.Black) == 0
        )
        {
            return _evaluator.Evaluate(board);
        }

        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        int moveCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount);

        if (depth > NullMoveReduction + 1 && moves[0].ForcedMovePriority == ForcedMovePriority.None)
        {
            NullMoveUndoState undo = board.MakeNullMove();
            int score = -Negamax(
                board,
                depth - 1 - NullMoveReduction,
                alpha: -beta,
                beta: -beta + 1
            );
            board.UndoNullMove(undo);

            if (score >= beta)
            {
                return beta;
            }
        }

        OrderMoves(board, depth, moves, moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            MoveUndoState undo = board.MakeMove(move);

            int score = -Negamax(board, depth - 1, alpha: -beta, beta: -alpha);

            board.UndoMove(undo);

            if (score > alpha)
            {
                alpha = score;
            }
            if (alpha >= beta)
            {
                if (move.CapturesMask == 0 && move.PromotesTo is null)
                {
                    _killerMoves[depth, 1] = _killerMoves[depth, 0];
                    _killerMoves[depth, 0] = move;
                }
                break;
            }
        }
        return alpha;
    }

    private void OrderMoves(BitBoard board, int depth, Span<BitMove> moves, int moveCount)
    {
        Span<int> scores = stackalloc int[moveCount];

        int write = 0;
        for (int i = 0; i < moveCount; i++)
        {
            int score = ScoreMove(moves[i], board, depth);
            scores[i] = score;

            if (score > 0 && i != write)
            {
                (moves[write], moves[i]) = (moves[i], moves[write]);
                write += 1;
            }
        }

        if (write > 1)
        {
            QuickSort(moves, scores, left: 0, right: write - 1);
        }
    }

    private static void QuickSort(Span<BitMove> moves, Span<int> scores, int left, int right)
    {
        if (left >= right)
        {
            return;
        }

        int pivotIndex = left + (right - left) / 2;
        int pivotValue = scores[pivotIndex];

        int i = left;
        int j = right;
        while (i <= j)
        {
            while (scores[i] > pivotValue)
            {
                i++;
            }
            while (scores[j] < pivotValue)
            {
                j--;
            }

            if (i <= j)
            {
                (moves[i], moves[j]) = (moves[j], moves[i]);
                (scores[i], scores[j]) = (scores[j], scores[i]);
                i++;
                j--;
            }
        }

        if (left < j)
        {
            QuickSort(moves, scores, left, j);
        }
        if (i < right)
        {
            QuickSort(moves, scores, i, right);
        }
    }

    private int ScoreMove(BitMove move, BitBoard board, int depth)
    {
        if (move.CapturesMask != 0)
        {
            UInt128 captureMask = move.CapturesMask;
            int score = 0;
            int attackerValue = MaterialEvaluator.GetPieceValue(move.Piece.Type);
            while (captureMask != 0)
            {
                byte captureSquare = (byte)BitboardHelpers.BitScanForward(ref captureMask);
                if (board.TryGetPieceAt(captureSquare, out var capturePiece))
                {
                    int victimValue = MaterialEvaluator.GetPieceValue(capturePiece.Value.Type);
                    score += victimValue - attackerValue;
                }
            }
            return 10_000 + score;
        }

        if (move.PromotesTo is not null)
        {
            return 9_000 + MaterialEvaluator.GetPieceValue(move.PromotesTo.Value);
        }

        if (move.SpecialMoveType is not SpecialMoveType.None)
        {
            return 8_000;
        }

        if (
            (move.From == _killerMoves[depth, 0].From && move.To == _killerMoves[depth, 0].To)
            || (move.From == _killerMoves[depth, 1].From && move.To == _killerMoves[depth, 1].To)
        )
        {
            return 7_000;
        }

        return 0;
    }
}
