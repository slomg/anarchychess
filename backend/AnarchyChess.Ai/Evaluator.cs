using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IEvaluator
{
    int EvaluateBoard(BitBoard board, Span<int> moveCountByPiece);
}

public class Evaluator : IEvaluator
{
    public int EvaluateBoard(BitBoard board, Span<int> moveCountByPiece)
    {
        BitPieceColor ourColor = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;

        int materialScore = CalculateMaterialScore(board, ourColor: ourColor);
        int mobilityScore = CalculateMobilityScore(moveCountByPiece);

        return materialScore + mobilityScore;
    }

    public static int GetPieceValue(PieceType type) =>
        type switch
        {
            PieceType.Queen => 900,
            PieceType.Pawn => 100,
            PieceType.Rook => 500,
            PieceType.Bishop => 300,
            PieceType.Horsey => 300,

            PieceType.Knook => 400,
            PieceType.Antiqueen => 300,
            PieceType.UnderagePawn => 150,
            PieceType.SterilePawn => 80,
            PieceType.Checker => 350,

            _ => 0,
        };

    private static int CalculateMaterialScore(BitBoard board, BitPieceColor ourColor)
    {
        int materialScore = 0;
        if (ourColor is BitPieceColor.White)
        {
            materialScore += board.WhiteMaterialCount - board.BlackMaterialCount;
            materialScore += EvaluateKingScore(board.WhiteKingCount);
            materialScore -= EvaluateKingScore(board.BlackKingCount);
        }
        else
        {
            materialScore += board.BlackMaterialCount - board.WhiteMaterialCount;
            materialScore += EvaluateKingScore(board.BlackKingCount);
            materialScore -= EvaluateKingScore(board.WhiteKingCount);
        }

        UInt128 traitorRookBitboard = board.BitboardFor(
            PieceType.TraitorRook,
            BitPieceColor.Neutral
        );
        while (traitorRookBitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref traitorRookBitboard);
            materialScore += EvaluateTraitorRookScore(board, ourColor, position);
        }

        return materialScore;
    }

    private static int CalculateMobilityScore(Span<int> moveCountByPiece)
    {
        int mobilityScore = 0;
        for (int i = 0; i < moveCountByPiece.Length; i++)
        {
            mobilityScore += moveCountByPiece[i] * GetPieceMobilityBonus((PieceType)i);
        }
        return mobilityScore;
    }

    private static int GetPieceMobilityBonus(PieceType type) =>
        type switch
        {
            PieceType.King => 0,
            PieceType.Queen => 1,
            PieceType.Pawn => 0,
            PieceType.Rook => 2,
            PieceType.Bishop => 5,
            PieceType.Horsey => 4,

            PieceType.Knook => 5,
            PieceType.Antiqueen => 4,
            PieceType.UnderagePawn => 1,
            PieceType.SterilePawn => 0,
            PieceType.Checker => 6,
            PieceType.TraitorRook => 0,

            _ => 0,
        };

    private static int EvaluateKingScore(int kingCount) =>
        kingCount > 0 ? 10_000 + (kingCount * 350) : 0;

    private static int EvaluateTraitorRookScore(
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
            return 0;
        }

        int ourAdjacentCount = BitboardHelpers.CountBits(ourAdjacent);
        int enemyAdjacentCount = BitboardHelpers.CountBits(enemyAdjacent);
        if (ourAdjacentCount > enemyAdjacentCount)
        {
            return 200;
        }
        else if (ourAdjacentCount == enemyAdjacentCount)
        {
            return 100;
        }
        else
        {
            return -200;
        }
    }
}
