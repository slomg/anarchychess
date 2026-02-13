using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class MaterialEvaluator
{
    public static int Evaluate(BitBoard board, BitPieceColor ourColor)
    {
        int materialScore = 0;
        if (ourColor is BitPieceColor.White)
        {
            materialScore += board.WhiteMaterialCount - board.BlackMaterialCount;
            materialScore += EvaluateKingValue(board.WhiteKingCount);
            materialScore -= EvaluateKingValue(board.BlackKingCount);
        }
        else
        {
            materialScore += board.BlackMaterialCount - board.WhiteMaterialCount;
            materialScore += EvaluateKingValue(board.BlackKingCount);
            materialScore -= EvaluateKingValue(board.WhiteKingCount);
        }

        UInt128 traitorRookBitboard = board.BitboardFor(
            PieceType.TraitorRook,
            BitPieceColor.Neutral
        );
        while (traitorRookBitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref traitorRookBitboard);
            materialScore += EvaluateTraitorRookValue(board, ourColor, position);
        }

        return materialScore;
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

    private static int EvaluateKingValue(int kingCount) =>
        kingCount > 0 ? 10_000 + (kingCount * 350) : 0;

    private static int EvaluateTraitorRookValue(
        BitBoard board,
        BitPieceColor ourColor,
        byte position
    )
    {
        UInt128 ourPieces = board.BitboardForFriendOf(ourColor);
        UInt128 enemyPieces = board.BitboardForEnemyOf(ourColor);

        UInt128 adjacent = PieceMasks.AdjacentMasks[position];
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
