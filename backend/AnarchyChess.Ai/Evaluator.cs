using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IEvaluator
{
    float EvaluateBoard(BitBoard board, Span<int> moveCountByPiece);
}

public class Evaluator : IEvaluator
{
    public float EvaluateBoard(BitBoard board, Span<int> moveCountByPiece)
    {
        BitPieceColor ourColor = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;

        float score;
        if (ourColor is BitPieceColor.White)
        {
            score = board.WhiteMaterialCount - board.BlackMaterialCount;
            score += EvaluateKingScore(board.WhiteKingCount);
            score -= EvaluateKingScore(board.BlackKingCount);
        }
        else
        {
            score = board.BlackMaterialCount - board.WhiteMaterialCount;
            score += EvaluateKingScore(board.BlackKingCount);
            score -= EvaluateKingScore(board.WhiteKingCount);
        }

        UInt128 traitorRookBitboard = board.BitboardFor(
            PieceType.TraitorRook,
            BitPieceColor.Neutral
        );
        while (traitorRookBitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref traitorRookBitboard);
            score += EvaluateTraitorRookScore(board, ourColor, position);
        }

        float activityBonus = 0f;
        for (int i = 0; i < moveCountByPiece.Length; i++)
        {
            activityBonus += moveCountByPiece[i] * GetPieceActivityBonus((PieceType)i);
        }

        return score + activityBonus;
    }

    public static float GetPieceValue(PieceType type) =>
        type switch
        {
            PieceType.Queen => 9f,
            PieceType.Pawn => 1f,
            PieceType.Rook => 5f,
            PieceType.Bishop => 3f,
            PieceType.Horsey => 3f,

            PieceType.Knook => 4f,
            PieceType.Antiqueen => 3f,
            PieceType.UnderagePawn => 1.5f,
            PieceType.SterilePawn => 0.8f,
            PieceType.Checker => 3.5f,

            _ => 0,
        };

    private static float GetPieceActivityBonus(PieceType type) =>
        type switch
        {
            PieceType.King => 0f,
            PieceType.Queen => 0f,
            PieceType.Pawn => 0f,
            PieceType.Rook => 0.02f,
            PieceType.Bishop => 0.06f,
            PieceType.Horsey => 0.07f,

            PieceType.Knook => 0.08f,
            PieceType.Antiqueen => 0.06f,
            PieceType.UnderagePawn => 0.005f,
            PieceType.SterilePawn => 0f,
            PieceType.Checker => 0.07f,
            PieceType.TraitorRook => 0.02f,

            _ => 0f,
        };

    private static float EvaluateKingScore(int kingCount) =>
        kingCount > 0 ? 10_000f + (kingCount * 3.5f) : 0;

    private static float EvaluateTraitorRookScore(
        BitBoard board,
        BitPieceColor ourColor,
        byte position
    )
    {
        UInt128 ourPieces = board.BitboardForFriendOf(ourColor);
        UInt128 enemyPieces = board.BitboardForEnemyOf(ourColor);

        UInt128 adjacent = BitboardHelpers.MaskAdjacent(position);
        UInt128 ourAdjacent = adjacent & ourPieces;
        UInt128 enemyAdjacent = adjacent & enemyPieces;

        if (ourAdjacent == 0 && enemyAdjacent == 0)
        {
            return 0f;
        }

        int ourAdjacentCount = BitboardHelpers.CountBits(ourAdjacent);
        int enemyAdjacentCount = BitboardHelpers.CountBits(enemyAdjacent);
        if (ourAdjacentCount > enemyAdjacentCount)
        {
            return 2f;
        }
        else if (ourAdjacentCount == enemyAdjacentCount)
        {
            return 1f;
        }
        else
        {
            return -2f;
        }
    }
}
