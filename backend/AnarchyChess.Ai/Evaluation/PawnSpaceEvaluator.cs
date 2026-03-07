using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public sealed class PawnSpaceEvaluator : IEvaluatorFunction
{
    public const int CenterAmplifier = 3;
    public const int PawnAdvanceValue = 4;

    public (int WhiteScore, int BlackScore) Evaluate(BitBoard board, float endgameFactor)
    {
        int whiteScore = EvaluatePawnSpace(
            board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
                | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White),
            targetRank: 9
        );

        int blackScore = EvaluatePawnSpace(
            board.BitboardFor(PieceType.Pawn, BitPieceColor.Black)
                | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black),
            targetRank: 0
        );

        return (WhiteScore: whiteScore, BlackScore: blackScore);
    }

    private static int EvaluatePawnSpace(UInt128 pawnBitboard, int targetRank)
    {
        int score = 0;

        while (pawnBitboard != 0)
        {
            byte square = (byte)BitboardHelpers.BitScanForward(ref pawnBitboard);

            int rank = square / 10;
            int file = square % 10;

            int distanceFromTarget = Math.Abs(rank - targetRank);
            int progressScore = 10 - distanceFromTarget;

            int centerAmplifier = file >= 3 && file <= 6 ? CenterAmplifier : 1;

            score += progressScore * PawnAdvanceValue * centerAmplifier;
        }

        return score;
    }
}
