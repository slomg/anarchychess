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

    private readonly IBitMoveGenerator _moveGenerator = moveGenerator ?? new BitMoveGenerator();
    private readonly IEvaluator _evaluator = evaluator ?? new Evaluator();

    private BitMove[,] _killerMoves = new BitMove[0, 0];

    const int MaxDepth = 16;

    private static readonly int[,] LmrTable = CreateLMR();

    static int[,] CreateLMR()
    {
        int[,] lm = new int[MaxDepth, MaxMoves];
        for (int depth = 1; depth < MaxDepth; depth++)
        {
            for (int move = 1; move < MaxMoves; move++)
            {
                double reduction = 0.99 + Math.Log(depth) * Math.Log(move) / 3.14;

                lm[depth, move] = Math.Max((int)Math.Round(reduction), 1);
            }
        }
        return lm;
    }

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

        Console.WriteLine($"Eval: {alpha}, move count: {moveCount}");

        return bestMove;
    }

    private int Negamax(
        BitBoard board,
        int depth,
        int alpha,
        int beta,
        bool isLastMoveCapture = false
    )
    {
        if (_evaluator.TryEvaluateTermination(board, depth, out int terminationEval))
        {
            return terminationEval;
        }

        if (depth <= 0)
        {
            return isLastMoveCapture
                ? Quiescence(board, alpha, beta, depth: 3, initialDepth: depth)
                : _evaluator.Evaluate(board);
        }

        if (depth < 3)
        {
            int standPat = _evaluator.Evaluate(board);
            int margin = 350;
            if (standPat + margin <= alpha)
            {
                return alpha;
            }
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

            int searchDepth = depth - 1;

            if (i > 0 && depth >= 3 && move.CapturesMask == 0 && move.PromotesTo is null)
            {
                int reduction = LmrTable[depth, i];
                searchDepth -= reduction;
            }

            int score = -Negamax(
                board,
                searchDepth,
                alpha: -beta,
                beta: -alpha,
                isLastMoveCapture = move.CapturesMask != 0
            );

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

    private int Quiescence(BitBoard board, int alpha, int beta, int depth, int initialDepth)
    {
        int standPat = _evaluator.Evaluate(board);

        if (standPat >= beta)
        {
            return beta;
        }
        if (standPat > alpha)
        {
            alpha = standPat;
        }

        if (
            _evaluator.TryEvaluateTermination(
                board,
                depth: initialDepth - depth,
                out int terminationEval
            )
        )
        {
            return terminationEval;
        }

        if (depth <= 0)
        {
            return alpha;
        }

        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        Span<BitMove> captures = stackalloc BitMove[50];
        int moveCount = 0;
        int captureCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount);

        int maxCaptureValue = 0;
        UInt128 pawns =
            board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
            | board.BitboardFor(PieceType.Pawn, BitPieceColor.Black);

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            if (move.CapturesMask != 0)
            {
                UInt128 capturesMask = move.CapturesMask;
                if (
                    board.TryGetPieceAt(
                        (byte)BitboardHelpers.BitScanForward(ref capturesMask),
                        out var piece
                    )
                )
                {
                    maxCaptureValue = Math.Max(
                        maxCaptureValue,
                        MaterialEvaluator.GetPieceValue(piece.Value.Type)
                    );
                }
                captures[captureCount++] = move;
            }
        }

        if (standPat + maxCaptureValue < alpha)
        {
            return alpha;
        }

        for (int i = 0; i < captureCount; i++)
        {
            BitMove move = captures[i];

            MoveUndoState undo = board.MakeMove(move);
            int score = -Quiescence(board, -beta, -alpha, depth - 1, initialDepth);
            board.UndoMove(undo);

            if (score >= beta)
            {
                return beta;
            }
            if (score > alpha)
            {
                alpha = score;
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
