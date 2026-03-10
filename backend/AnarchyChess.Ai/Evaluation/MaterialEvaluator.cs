using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class MaterialEvaluator : IEvaluatorFunction
{
    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        int whiteScore = board.WhiteMaterialCount;
        int blackScore = board.BlackMaterialCount;

        UInt128 traitorRookBitboard = board.BitboardFor(
            PieceType.TraitorRook,
            BitPieceColor.Neutral
        );
        while (traitorRookBitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref traitorRookBitboard);
            EvaluateTraitorRookValue(board, position, ref whiteScore, ref blackScore);
        }

        return (WhiteScore: whiteScore, BlackScore: blackScore);
    }

    private static void EvaluateTraitorRookValue(
        BitBoard board,
        byte position,
        ref int whiteScore,
        ref int blackScore
    )
    {
        UInt128 adjacent = PieceMasks.AdjacentMasks[position];
        UInt128 whiteAdjacent = adjacent & board.WhitePieces;
        UInt128 blackAdjacent = adjacent & board.BlackPieces;

        if (whiteAdjacent == 0 && blackAdjacent == 0)
        {
            if (position < 50)
            {
                whiteScore += 150;
            }
            else
            {
                blackScore += 150;
            }
            return;
        }

        int whiteAdjacentCount = BitboardHelpers.CountBits(whiteAdjacent);
        int blackAdjacentCount = BitboardHelpers.CountBits(blackAdjacent);

        if (whiteAdjacentCount > blackAdjacentCount)
        {
            whiteScore += 150;
            return;
        }
        else if (blackAdjacentCount > whiteAdjacentCount)
        {
            blackScore += 150;
            return;
        }

        if (position < 50)
        {
            whiteScore += 150;
        }
        else
        {
            blackScore += 150;
        }
    }
}
