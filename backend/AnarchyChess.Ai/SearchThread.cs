using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

internal class SearchThread(
    IBitMoveGenerator moveGenerator,
    IEvaluator evaluator,
    IMoveOrdering moveOrdering,
    int depth
)
{
    public int Score { get; private set; }

    private readonly IBitMoveGenerator _moveGenerator = moveGenerator;
    private readonly IEvaluator _evaluator = evaluator;
    private readonly IMoveOrdering _moveOrdering = moveOrdering;

    private readonly BitMove[,] _killerMoves = new BitMove[depth + 1, 2];
    private readonly int[,] _historyHeuristic = new int[10 * 10, 10 * 10];

    public int Negamax(
        BitBoard board,
        int depth,
        int alpha,
        int beta,
        bool isLastMoveCapture = false,
        bool isLastMoveForced = false
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

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount);

        if (
            depth < 3
            && moves[0].ForcedMovePriority is ForcedMovePriority.None
            && _evaluator.Evaluate(board) + EngineConstants.FutilityMargin <= alpha
        )
        {
            return alpha;
        }

        if (
            depth > EngineConstants.NullMoveReduction + 1
            && moves[0].ForcedMovePriority is ForcedMovePriority.None
        )
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

        _moveOrdering.OrderMoves(board, depth, _killerMoves, _historyHeuristic, moves, moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            MoveUndoState undo = board.MakeMove(move);

            int searchDepth = depth - 1;

            if (
                i > 0
                && depth >= 3
                && move.CapturesMask == 0
                && move.PromotesTo is null
                && move.ForcedMovePriority is ForcedMovePriority.None
                && !isLastMoveForced
            )
            {
                int reduction = EngineConstants.LmrTable[depth, i];
                searchDepth -= reduction;
            }

            int score = -Negamax(
                board,
                searchDepth,
                alpha: -beta,
                beta: -alpha,
                isLastMoveCapture = move.CapturesMask != 0,
                isLastMoveForced = move.ForcedMovePriority is not ForcedMovePriority.None
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
                    _historyHeuristic[move.From, move.To] += depth * depth;
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

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        Span<BitMove> captures = stackalloc BitMove[50];
        int moveCount = 0;
        int captureCount = 0;
        _moveGenerator.Generate(board, moves, ref moveCount);

        int maxCaptureValue = 0;
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

        if (
            standPat + maxCaptureValue < alpha
            && moves[0].ForcedMovePriority is ForcedMovePriority.None
        )
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
}
