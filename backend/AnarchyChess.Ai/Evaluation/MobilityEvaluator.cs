using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class MobilityEvaluator
{
    public static int Evaluate(BitBoard board, BitPieceColor ourColor, BitPieceColor enemyColor)
    {
        int mobilityScore = 0;
        mobilityScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Bishop,
            ourColor,
            enemyColor
        );
        mobilityScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Rook,
            ourColor,
            enemyColor
        );
        mobilityScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Horsey,
            ourColor,
            enemyColor
        );
        mobilityScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Antiqueen,
            ourColor,
            enemyColor
        );
        mobilityScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Knook,
            ourColor,
            enemyColor
        );
        return mobilityScore;
    }

    private static int CalculateMobilityScoreForPiece(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor ourColor,
        BitPieceColor enemyColor
    )
    {
        UInt128 ourBitboard = board.BitboardFor(pieceType, ourColor);
        UInt128 enemyBitboard = board.BitboardFor(pieceType, enemyColor);

        UInt128 enemyPieces = board.BitboardForEnemyOf(ourColor);
        UInt128 ourPieces = board.BitboardForEnemyOf(enemyColor);

        int mobilityBonus = GetPieceMobilityBonus(pieceType);

        int mobilityScore = 0;
        while (ourBitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref ourBitboard);
            UInt128 attacks = GetEvalMaskForPiece(pieceType, position, board.Occupancy);
            UInt128 quiets = attacks & board.Empty;
            UInt128 captures = attacks & enemyPieces;

            mobilityScore += BitboardHelpers.CountBits(quiets) * mobilityBonus;
            mobilityScore += BitboardHelpers.CountBits(captures) * (mobilityBonus + 1);
        }

        while (enemyBitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref enemyBitboard);
            UInt128 attacks = GetEvalMaskForPiece(pieceType, position, board.Occupancy);
            UInt128 quiets = attacks & board.Empty;
            UInt128 captures = attacks & ourPieces;

            mobilityScore -= BitboardHelpers.CountBits(quiets) * mobilityBonus;
            mobilityScore -= BitboardHelpers.CountBits(captures) * (mobilityBonus + 1);
        }

        return mobilityScore;
    }

    private static int GetPieceMobilityBonus(PieceType type) =>
        type switch
        {
            PieceType.Rook => 2,
            PieceType.Bishop => 5,
            PieceType.Horsey => 4,
            PieceType.Knook => 5,
            PieceType.Antiqueen => 4,

            _ => 0,
        };

    private static UInt128 GetEvalMaskForPiece(PieceType piece, byte position, UInt128 occupancy) =>
        piece switch
        {
            PieceType.Horsey => PieceMasks.HorseyMasks[position],
            PieceType.Antiqueen => PieceMasks.HorseyMasks[position],
            PieceType.Rook => MagicLibrary.GetAttacks(MagicLibrary.RookTable, position, occupancy),
            PieceType.Bishop => MagicLibrary.GetAttacks(
                MagicLibrary.BishopTable,
                position,
                occupancy
            ),
            PieceType.Knook => PieceMasks.HorseyMasks[position]
                | MagicLibrary.GetAttacks(
                    MagicLibrary.TwoStraightSquaresTable,
                    position,
                    occupancy
                ),

            _ => 0,
        };
}
