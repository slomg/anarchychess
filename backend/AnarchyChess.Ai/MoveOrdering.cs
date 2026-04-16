using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IMoveOrdering
{
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
    void SortMoves(
        BitBoard board,
        int depth,
        BitMove[,] killers,
        int[,] history,
        Span<BitMove> moves,
        int moveCount
    );
}

public sealed class MoveOrdering : IMoveOrdering
{
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

        if (move.SpecialMoveType is not SpecialMoveType.Throw && move.CapturesMask != 0)
        {
            UInt128 captureMask = move.CapturesMask;
            int score = 0;
            int attackerValue = MaterialValue.GetPieceValue(move.Piece.Type);
            while (captureMask != 0)
            {
                byte captureSquare = (byte)BitboardHelpers.BitScanForward(ref captureMask);
                if (board.TryGetPieceAt(captureSquare, out var capturePiece))
                {
                    int victimValue = MaterialValue.GetPieceValue(capturePiece.Value.Type);
                    score += victimValue - attackerValue;
                }
            }
            return 10_000 + score;
        }

        if (move.PromotesTo is not null)
        {
            return 8_000 + MaterialValue.GetPieceValue(move.PromotesTo.Value);
        }

        if (
            move.SpecialMoveType is SpecialMoveType.Throw
            && move.CapturesMask != 0
            && board.TryGetPieceAt(move.To, out var stunnedPiece)
        )
        {
            int value = MaterialValue.GetPieceValue(stunnedPiece.Value.Type);
            return 7_000 + value;
        }

        if (move.SpecialMoveType is SpecialMoveType.Throw)
        {
            return 6_000 + historyHeuristic[move.From, move.To];
        }

        return historyHeuristic[move.From, move.To];
    }

    public void SortMoves(
        BitBoard board,
        int depth,
        BitMove[,] killers,
        int[,] history,
        Span<BitMove> moves,
        int moveCount
    )
    {
        Span<int> scores = stackalloc int[moveCount];
        ScoreMoves(board, depth, killers, history, scores, moves, moveCount);

        QuickSort(moves, scores, 0, moveCount - 1);
    }

    private static void QuickSort(Span<BitMove> moves, Span<int> scores, int left, int right)
    {
        while (left < right)
        {
            int i = left;
            int j = right;
            int pivot = scores[(left + right) >> 1];

            while (i <= j)
            {
                while (scores[i] > pivot)
                {
                    i++;
                }
                while (scores[j] < pivot)
                {
                    j--;
                }

                if (i <= j)
                {
                    (scores[i], scores[j]) = (scores[j], scores[i]);
                    (moves[i], moves[j]) = (moves[j], moves[i]);
                    i++;
                    j--;
                }
            }

            if (j - left < right - i)
            {
                if (left < j)
                {
                    QuickSort(moves, scores, left, j);
                }
                left = i;
            }
            else
            {
                if (i < right)
                {
                    QuickSort(moves, scores, i, right);
                }
                right = j;
            }
        }
    }
}
