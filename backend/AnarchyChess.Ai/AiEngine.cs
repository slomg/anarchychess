using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai;

public interface IAiEngine
{
    MoveEvaluation[] EvaluateAllMoves(BitBoard board, int depth);
    (BitMove? BestMove, int EvalForBot) FindBestMove(BitBoard board, int depth);
}

public class AiEngine : IAiEngine
{
    private static readonly TranspositionTable _transpositionTable = new();

    public (BitMove? BestMove, int EvalForBot) FindBestMove(BitBoard board, int depth)
    {
        int score = 0;
        BitMove? bestMove = null;

        for (int iterativeDepth = 1; iterativeDepth <= depth; iterativeDepth++)
        {
            int aspirationDelta = 50;
            int aspirationAlpha;
            int aspirationBeta;
            if (iterativeDepth < 4)
            {
                aspirationAlpha = EngineConstants.AlphaStart;
                aspirationBeta = EngineConstants.BetaStart;
            }
            else
            {
                aspirationAlpha = score - aspirationDelta;
                aspirationBeta = score + aspirationDelta;
            }

            while (true)
            {
                (bestMove, score) = SearchRoot(
                    board,
                    iterativeDepth,
                    aspirationAlpha,
                    aspirationBeta
                );

                if (score <= aspirationAlpha)
                {
                    aspirationAlpha -= aspirationDelta;
                    aspirationDelta *= 2;
                }
                else if (score >= aspirationBeta)
                {
                    aspirationBeta += aspirationDelta;
                    aspirationDelta *= 2;
                }
                else
                {
                    break;
                }
            }
        }

        return (bestMove, score);
    }

    public (BitMove? BestMove, int EvalForBot) SearchRoot(
        BitBoard board,
        int depth,
        int alpha,
        int beta
    )
    {
        BitMove[] moves = new BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;

        BitMoveGenerator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: depth);
        if (moveCount == 0)
        {
            return (BestMove: null, EvalForBot: 0);
        }

        MoveOrdering.SortMoves(
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

        alpha = -new SearchThread(_transpositionTable, depth).Negamax(
            olderBoardCopy,
            depth - 1,
            alpha: -beta,
            beta: -alpha
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

                SearchThread search = new(_transpositionTable, depth);

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

        return (BestMove: bestMove, EvalForBot: bestAlpha);
    }

    public MoveEvaluation[] EvaluateAllMoves(BitBoard board, int depth)
    {
        BitMove[] moves = new BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;

        BitMoveGenerator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: depth);
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

                SearchThread search = new(_transpositionTable, depth);

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
