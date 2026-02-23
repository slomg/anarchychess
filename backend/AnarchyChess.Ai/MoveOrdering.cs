using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IMoveOrdering
{
    BitMove SelectAndPromoteHighestMove(
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        int[,] historyHeuristic,
        Span<BitMove> moves,
        int moveCount
    );
    void ScoreMoves(
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        int[,] historyHeuristic,
        Span<int> scores,
        Span<BitMove> moves,
        int moveCount
    );
    BitMove GetNextHighestMove(int i, Span<BitMove> moves, Span<int> scores, int moveCount);
}

public sealed class MoveOrdering : IMoveOrdering
{
    public BitMove SelectAndPromoteHighestMove(
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        int[,] historyHeuristic,
        Span<BitMove> moves,
        int moveCount
    )
    {
        int bestScore = 0;
        BitMove bestMove = default;
        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            int score = ScoreMove(move, board, depth, killerMoves, historyHeuristic);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
                (moves[0], moves[i]) = (moves[i], moves[0]);
            }
        }
        return bestMove;
    }

    public void ScoreMoves(
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        int[,] historyHeuristic,
        Span<int> scores,
        Span<BitMove> moves,
        int moveCount
    )
    {
        for (int i = 0; i < moveCount; i++)
        {
            scores[i] = ScoreMove(moves[i], board, depth, killerMoves, historyHeuristic);
        }
    }

    public BitMove GetNextHighestMove(int i, Span<BitMove> moves, Span<int> scores, int moveCount)
    {
        int bestIndex = i;
        int bestScore = scores[i];

        for (int j = i + 1; j < moveCount; j++)
        {
            if (scores[j] > bestScore)
            {
                bestScore = scores[j];
                bestIndex = j;
            }
        }

        if (bestIndex != i)
        {
            (moves[i], moves[bestIndex]) = (moves[bestIndex], moves[i]);
            (scores[i], scores[bestIndex]) = (scores[bestIndex], scores[i]);
        }
        return moves[i];
    }

    private static int ScoreMove(
        BitMove move,
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        int[,] historyHeuristic
    )
    {
        if (
            (move.From == killerMoves[depth, 0].From && move.To == killerMoves[depth, 0].To)
            || (move.From == killerMoves[depth, 1].From && move.To == killerMoves[depth, 1].To)
        )
        {
            return 15_000;
        }

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
            return 8_000 + MaterialEvaluator.GetPieceValue(move.PromotesTo.Value);
        }

        if (move.SpecialMoveType is not SpecialMoveType.None)
        {
            return 7_000;
        }

        return historyHeuristic[move.From, move.To];
    }
}
