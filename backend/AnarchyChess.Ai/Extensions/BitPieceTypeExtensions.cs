using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Extensions;

public static class BitPieceTypeExtensions
{
    public static GameColor? Color(this BitPieceType pieceType)
    {
        if (pieceType.IsWhite())
        {
            return GameColor.White;
        }
        else if (pieceType.IsBlack())
        {
            return GameColor.Black;
        }
        return null;
    }

    public static bool IsWhite(this BitPieceType pieceType) =>
        pieceType > BitPieceType.WHITE_START_MARKER && pieceType < BitPieceType.WHITE_END_MARKER;

    public static bool IsBlack(this BitPieceType pieceType) =>
        pieceType > BitPieceType.BLACK_START_MARKER && pieceType < BitPieceType.BLACK_END_MARKER;

    public static bool IsNeutral(this BitPieceType pieceType) =>
        pieceType > BitPieceType.NEUTRAL_START_MARKER
        && pieceType < BitPieceType.NEUTRAL_END_MARKER;
}
