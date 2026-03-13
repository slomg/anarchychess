using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Evaluation;

public static class EndgameFactorCalculator
{
    public const int MaxPhase = 17;

    public static float EndgameFactor(BitBoard board)
    {
        float queenCount = BitboardHelpers.CountBits(
            board.BitboardFor(PieceType.Queen, BitPieceColor.White)
                | board.BitboardFor(PieceType.Queen, BitPieceColor.Black)
        );

        float rookCount = BitboardHelpers.CountBits(
            board.BitboardFor(PieceType.Rook, BitPieceColor.White)
                | board.BitboardFor(PieceType.Rook, BitPieceColor.Black)
        );

        float knookCount = BitboardHelpers.CountBits(
            board.BitboardFor(PieceType.Knook, BitPieceColor.White)
                | board.BitboardFor(PieceType.Knook, BitPieceColor.Black)
        );

        float phase = (queenCount * 4) + (rookCount * 2) + (knookCount * 0.5f);

        return 1 - Math.Clamp(phase / MaxPhase, 0f, 1f);
    }
}
