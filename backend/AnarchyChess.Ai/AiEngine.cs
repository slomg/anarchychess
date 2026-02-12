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

    public BitMove? FindBestMove(BitBoard board, int depth)
    {
        BitMove[] moves = new BitMove[MaxMoves];
        int moveCount = 0;

        _moveGenerator.Generate(board, moves, ref moveCount);
        OrderMove(board, moves, moveCount);

        if (moveCount == 0)
            return null;

        BitMove bestMove = moves[0];
        BitBoard boardCopy = new(board);
        boardCopy.MakeMove(bestMove);

        float alpha = -Negamax(boardCopy, depth - 1, alpha: ALPHA_START, beta: BETA_START);

        float[] scores = new float[moveCount];

        Parallel.For(
            1,
            moveCount,
            i =>
            {
                BitMove move = moves[i];
                BitBoard boardCopy = new(board);
                boardCopy.MakeMove(move);

                float score = -Negamax(boardCopy, depth - 1, alpha: alpha, beta: BETA_START);

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

        Console.WriteLine("Eval: " + alpha);

        return bestMove;
    }

    private float Negamax(BitBoard board, int depth, float alpha, float beta)
    {
        if (depth <= 0)
        {
            return _evaluator.EvaluateBoard(board);
        }

        if (depth > NullMoveReduction + 1)
        {
            NullMoveUndoState undo = board.MakeNullMove();
            float score = -Negamax(
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

        Span<BitMove> moves = stackalloc BitMove[MaxMoves];
        int moveCount = 0;

        _moveGenerator.Generate(board, moves, ref moveCount);
        OrderMove(board, moves, moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            MoveUndoState undo = board.MakeMove(move);

            float score = -Negamax(board, depth - 1, alpha: -beta, beta: -alpha);

            board.UndoMove(undo);

            if (score > alpha)
            {
                alpha = score;
            }
            if (alpha >= beta)
            {
                break;
            }
        }
        return alpha;
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
            int attackerValue = (int)Evaluator.GetPieceValue(move.Piece.Type);
            while (captureMask != 0)
            {
                byte captureSquare = (byte)BitboardHelpers.BitScanForward(ref captureMask);
                if (board.TryGetPieceAt(captureSquare, out var capturePiece))
                {
                    int victimValue = (int)Evaluator.GetPieceValue(capturePiece.Value.Type);
                    score += victimValue * 100 - attackerValue;
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
