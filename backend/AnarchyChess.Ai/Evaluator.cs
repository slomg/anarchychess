using AnarchyChess.Ai.Helpers;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IEvaluator
{
    float EvaluateBoard(BitBoard board, BitPieceColor ourColor);
}

public class Evaluator : IEvaluator
{
    public float EvaluateBoard(BitBoard board, BitPieceColor ourColor)
    {
        float score = 0;

        for (int colorIdx = 0; colorIdx < board.Bitboards.GetLength(0); colorIdx++)
        {
            BitPieceColor pieceColor = (BitPieceColor)colorIdx;

            for (int pieceTypeIdx = 0; pieceTypeIdx < board.Bitboards.GetLength(1); pieceTypeIdx++)
            {
                PieceType pieceType = (PieceType)pieceTypeIdx;

                UInt128 bitboard = board.BitboardFor(pieceType, pieceColor);
                while (bitboard != 0)
                {
                    byte squareIndex = (byte)BitboardHelpers.BitScanForward(ref bitboard);

                    float value = GetPieceValue(
                        board,
                        ourColor: ourColor,
                        pieceType,
                        pieceColor: pieceColor,
                        squareIndex
                    );

                    if (pieceColor is BitPieceColor.Neutral || pieceColor == ourColor)
                    {
                        score += value;
                    }
                    else
                    {
                        score -= value;
                    }
                }
            }
        }

        return score;
    }

    private static float GetPieceValue(
        BitBoard board,
        BitPieceColor ourColor,
        PieceType type,
        BitPieceColor pieceColor,
        byte position
    ) =>
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

            PieceType.King => EvaluateKingScore(board, pieceColor: pieceColor),
            PieceType.TraitorRook => EvaluateTraitorRookScore(board, ourColor: ourColor, position),

            _ => 0,
        };

    private static float EvaluateKingScore(BitBoard board, BitPieceColor pieceColor)
    {
        UInt128 kings = board.BitboardFor(PieceType.King, pieceColor);
        int kingCount = BitboardHelpers.CountBits(kings);

        float singleKingValue = 10_000f / kingCount;

        return singleKingValue + 3.5f;
    }

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
            return 0;
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
