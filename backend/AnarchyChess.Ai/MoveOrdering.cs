using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IMoveOrdering
{
    void OrderMoves(
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        Span<BitMove> moves,
        int moveCount
    );
}

public sealed class MoveOrdering : IMoveOrdering
{
    public void OrderMoves(
        BitBoard board,
        int depth,
        BitMove[,] killerMoves,
        Span<BitMove> moves,
        int moveCount
    )
    {
        Span<int> scores = stackalloc int[moveCount];

        int write = 0;
        for (int i = 0; i < moveCount; i++)
        {
            int score = ScoreMove(moves[i], board, depth, killerMoves);

            scores[i] = score;

            if (score > 0 && i != write)
            {
                (moves[write], moves[i]) = (moves[i], moves[write]);
                (scores[write], scores[i]) = (scores[i], scores[write]);
            }
            if (score > 0)
            {
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

    private static int ScoreMove(BitMove move, BitBoard board, int depth, BitMove[,] killerMoves)
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
            return 8_000 + MaterialEvaluator.GetPieceValue(move.PromotesTo.Value);
        }

        if (move.SpecialMoveType is not SpecialMoveType.None)
        {
            return 7_000;
        }

        if (
            (move.From == killerMoves[depth, 0].From && move.To == killerMoves[depth, 0].To)
            || (move.From == killerMoves[depth, 1].From && move.To == killerMoves[depth, 1].To)
        )
        {
            return 6_000;
        }

        return 0;
    }
}
