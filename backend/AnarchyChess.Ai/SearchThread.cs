using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

internal sealed class SearchThread(
    IBitMoveGenerator moveGenerator,
    IEvaluator evaluator,
    IMoveOrdering moveOrdering,
    int maxDepth
)
{
    private readonly IBitMoveGenerator _moveGenerator = moveGenerator;
    private readonly IEvaluator _evaluator = evaluator;
    private readonly IMoveOrdering _moveOrdering = moveOrdering;
    private readonly int _maxDepth = maxDepth;

    private readonly BitMove[,] _killerMoves = new BitMove[maxDepth + 1, 2];
    private readonly int[,] _historyHeuristic = new int[10 * 10, 10 * 10];

    public int Negamax(
        BitBoard board,
        int depth,
        int alpha,
        int beta,
        bool isLastMoveCapture = false,
        int? depthBeforeReduce = null
    )
    {
        if (_evaluator.TryEvaluateTermination(board, depth, out int terminationEval))
        {
            return terminationEval;
        }

        if (depth <= 0)
        {
            return isLastMoveCapture
                ? Quiescence(board, alpha, beta, depth: 4, initialDepth: depth)
                : _evaluator.Evaluate(board);
        }

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: _maxDepth);

        bool isForced = moves[0].ForcedMovePriority is not ForcedMovePriority.None;
        if (isForced && depthBeforeReduce is not null)
        {
            depth = depthBeforeReduce.Value;
        }

        if (depth > EngineConstants.NullMoveReduction + 1 && !isForced)
        {
            NullMoveUndoState undo = board.MakeNullMove();
            int score = -Negamax(
                board,
                depth - 1 - EngineConstants.NullMoveReduction,
                alpha: -beta,
                beta: -beta + 1
            );
            board.UndoNullMove(undo);

            if (score >= beta)
            {
                return beta;
            }
        }

        Span<int> scores = stackalloc int[moveCount];
        _moveOrdering.ScoreMoves(
            board,
            depth,
            _killerMoves,
            _historyHeuristic,
            scores,
            moves,
            moveCount
        );

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = _moveOrdering.GetNextHighestMove(i, moves, scores, moveCount);
            bool isCapture = move.CapturesMask != 0;
            bool isQuiet = !isCapture && !isForced && move.PromotesTo is null;

            MoveUndoState undo = board.MakeMove(move);

            int searchDepth = depth - 1;
            bool reduce = i > 0 && depth >= 3 && isQuiet;
            if (reduce)
            {
                searchDepth -= EngineConstants.LmrTable[depth, i];
            }

            int score = -Negamax(
                board,
                searchDepth,
                alpha: -beta,
                beta: -alpha,
                isLastMoveCapture: isCapture,
                depthBeforeReduce: depth - 1
            );

            if (reduce && score > alpha)
            {
                score = -Negamax(
                    board,
                    depth - 1,
                    -beta,
                    -alpha,
                    isLastMoveCapture: isCapture,
                    depthBeforeReduce: depth - 1
                );
            }

            board.UndoMove(undo);

            if (score > alpha)
            {
                alpha = score;
            }
            if (alpha >= beta)
            {
                _killerMoves[depth, 1] = _killerMoves[depth, 0];
                _killerMoves[depth, 0] = move;
                if (move.CapturesMask == 0 && move.PromotesTo is null)
                {
                    _historyHeuristic[move.From, move.To] += depth * depth;
                }
                break;
            }
        }

        return alpha;
    }

    private int Quiescence(BitBoard board, int alpha, int beta, int depth, int initialDepth)
    {
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

        int standPat = _evaluator.Evaluate(board);

        if (standPat >= beta)
        {
            return beta;
        }
        if (standPat > alpha)
        {
            alpha = standPat;
        }

        if (depth <= 0)
        {
            return alpha;
        }

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: _maxDepth);

        int captureCount = 0;
        Span<BitMove> captures = stackalloc BitMove[moveCount];
        int maxCaptureValue = 0;
        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            if (move.CapturesMask == 0)
            {
                continue;
            }

            UInt128 capturesMask = move.CapturesMask;
            byte firstCaptureAt = (byte)BitboardHelpers.BitScanForward(ref capturesMask);
            BitPiece? capturedPiece = board.GetPieceAt(firstCaptureAt);
            if (capturedPiece is null)
            {
                continue;
            }

            int captureValue = MaterialValue.GetPieceValue(capturedPiece.Value.Type);
            maxCaptureValue = Math.Max(maxCaptureValue, captureValue);
            if (standPat + captureValue + EngineConstants.DeltaPruningMargin < alpha)
            {
                continue;
            }

            captures[captureCount++] = move;
        }

        if (standPat + maxCaptureValue + EngineConstants.DeltaPruningMargin < alpha)
        {
            return alpha;
        }

        Span<int> scores = stackalloc int[captureCount];
        _moveOrdering.ScoreMoves(
            board,
            initialDepth,
            _killerMoves,
            _historyHeuristic,
            scores,
            captures,
            captureCount
        );

        for (int i = 0; i < captureCount; i++)
        {
            BitMove move = _moveOrdering.GetNextHighestMove(i, captures, scores, captureCount);

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
}
