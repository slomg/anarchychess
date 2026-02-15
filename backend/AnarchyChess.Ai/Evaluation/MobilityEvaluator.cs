using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class MobilityEvaluator : IEvaluatorFunction
{
    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        int whiteScore = 0;
        int blackScore = 0;

        whiteScore += CalculateMobilityScoreForPiece(board, PieceType.Bishop, BitPieceColor.White);
        blackScore += CalculateMobilityScoreForPiece(board, PieceType.Bishop, BitPieceColor.Black);

        whiteScore += CalculateMobilityScoreForPiece(board, PieceType.Rook, BitPieceColor.White);
        blackScore += CalculateMobilityScoreForPiece(board, PieceType.Rook, BitPieceColor.Black);

        whiteScore += CalculateMobilityScoreForPiece(board, PieceType.Horsey, BitPieceColor.White);
        blackScore += CalculateMobilityScoreForPiece(board, PieceType.Horsey, BitPieceColor.Black);

        whiteScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Antiqueen,
            BitPieceColor.White
        );
        blackScore += CalculateMobilityScoreForPiece(
            board,
            PieceType.Antiqueen,
            BitPieceColor.Black
        );

        whiteScore += CalculateMobilityScoreForPiece(board, PieceType.Knook, BitPieceColor.White);
        blackScore += CalculateMobilityScoreForPiece(board, PieceType.Knook, BitPieceColor.Black);

        return (WhiteScore: whiteScore, BlackScore: blackScore);
    }

    private static int CalculateMobilityScoreForPiece(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color
    )
    {
        int mobilityBonus = GetPieceMobilityBonus(pieceType);

        int mobilityScore = 0;
        UInt128 bitboard = board.BitboardFor(pieceType, color);
        UInt128 enemyPieces = board.BitboardForEnemyOf(color);
        while (bitboard != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref bitboard);
            UInt128 attacks = GetEvalMaskForPiece(pieceType, position, board.Occupancy);
            UInt128 quiets = attacks & board.Empty;
            UInt128 captures = attacks & enemyPieces;

            mobilityScore += BitboardHelpers.CountBits(quiets) * mobilityBonus;
            mobilityScore += BitboardHelpers.CountBits(captures) * (mobilityBonus + 1);
        }

        return mobilityScore;
    }

    private static int GetPieceMobilityBonus(PieceType type) =>
        type switch
        {
            PieceType.Rook => 1,
            PieceType.Bishop => 2,
            PieceType.Horsey => 2,
            PieceType.Knook => 2,
            PieceType.Antiqueen => 2,

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
