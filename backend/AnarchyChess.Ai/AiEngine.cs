using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IAiEngine
{
    MoveEvaluation[] EvaluateAllMoves(BitBoard board, int depth);
    (BitMove? BestMove, int EvalForBot) FindBestMove(BitBoard board, int depth);
}

public class AiEngine(
    IBitMoveGenerator? moveGenerator = null,
    IEvaluator? evaluator = null,
    IMoveOrdering? moveOrdering = null
) : IAiEngine
{
    private readonly IBitMoveGenerator _moveGenerator = moveGenerator ?? new BitMoveGenerator();
    private readonly IEvaluator _evaluator = evaluator ?? new Evaluator();
    private readonly IMoveOrdering _moveOrdering = moveOrdering ?? new MoveOrdering();

    public (BitMove? BestMove, int EvalForBot) FindBestMove(BitBoard board, int depth)
    {
        BitMove[] moves = new BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;

        _moveGenerator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: depth);
        if (moveCount == 0)
        {
            return (BestMove: null, EvalForBot: 0);
        }

        _moveOrdering.SortMoves(
            board,
            depth,
            new BitMove[depth + 1, 2],
            new int[10 * 10, 10 * 10],
            moves,
            moveCount
        );

        BitBoard olderBoardCopy = new(board);
        BitMove olderMove = moves[0];
        olderBoardCopy.MakeMove(olderMove);

        int alpha = -new SearchThread(_moveGenerator, _evaluator, _moveOrdering, depth).Negamax(
            olderBoardCopy,
            depth - 1,
            alpha: EngineConstants.AlphaStart,
            beta: EngineConstants.BetaStart
        );

        int[] scores = new int[moveCount];
        scores[0] = alpha;

        Parallel.For(
            1,
            moveCount,
            new() { MaxDegreeOfParallelism = 4 },
            i =>
            {
                BitMove move = moves[i];
                BitBoard boardCopy = new(board);
                boardCopy.MakeMove(move);

                SearchThread search = new(_moveGenerator, _evaluator, _moveOrdering, depth);

                int localAlpha = alpha;
                int score = -search.Negamax(
                    boardCopy,
                    depth - 1,
                    alpha: -localAlpha - 1,
                    beta: -localAlpha
                );

                if (score > localAlpha)
                {
                    score = -search.Negamax(
                        boardCopy,
                        depth - 1,
                        alpha: EngineConstants.AlphaStart,
                        beta: EngineConstants.BetaStart
                    );
                    scores[i] = score;
                }
                else
                {
                    scores[i] = int.MinValue;
                }

                // maybe I'll add this someday
                //int score = 0;
                //for (int iterativeDepth = 1; iterativeDepth <= depth - 1; iterativeDepth++)
                //{
                //    score = -search.Negamax(
                //        boardCopy,
                //        iterativeDepth,
                //        alpha: -alpha - 1,
                //        beta: -alpha
                //    );

                //    if (score > alpha)
                //    {
                //        score = -search.Negamax(
                //            boardCopy,
                //            iterativeDepth,
                //            alpha: EngineConstants.AlphaStart,
                //            beta: EngineConstants.BetaStart
                //        );
                //    }
                //}

                int oldAlpha;
                do
                {
                    oldAlpha = alpha;
                    if (score <= oldAlpha)
                    {
                        break;
                    }
                } while (Interlocked.CompareExchange(ref alpha, score, oldAlpha) != oldAlpha);
            }
        );

        BitMove bestMove = moves[0];
        int bestAlpha = scores[0];
        for (int i = 1; i < moveCount; i++)
        {
            if (scores[i] > bestAlpha)
            {
                bestAlpha = scores[i];
                bestMove = moves[i];
            }
        }

        Console.WriteLine(
            $"Eval: {bestAlpha}, move count: {moveCount}, {AlgebraicPoint.FromIdx(bestMove.From)} -> {AlgebraicPoint.FromIdx(bestMove.To)}"
        );

        return (BestMove: bestMove, EvalForBot: bestAlpha);
    }

    public MoveEvaluation[] EvaluateAllMoves(BitBoard board, int depth)
    {
        BitMove[] moves = new BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;

        _moveGenerator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: depth);
        if (moveCount == 0)
        {
            return [];
        }

        MoveEvaluation[] moveScores = new MoveEvaluation[moveCount];
        Parallel.For(
            0,
            moveCount,
            new() { MaxDegreeOfParallelism = 4 },
            i =>
            {
                BitMove move = moves[i];
                BitBoard boardCopy = new(board);
                boardCopy.MakeMove(move);

                SearchThread search = new(_moveGenerator, _evaluator, _moveOrdering, depth);

                int score = -search.Negamax(
                    boardCopy,
                    depth - 1,
                    alpha: EngineConstants.AlphaStart,
                    beta: EngineConstants.BetaStart
                );
                moveScores[i] = new MoveEvaluation(move, score);
            }
        );

        return moveScores;
    }
}
