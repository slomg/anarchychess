using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Constants;

public static class BitPieceMap
{
    public static readonly IReadOnlyDictionary<PieceType, BitPiece> Colored = new Dictionary<
        PieceType,
        BitPiece
    >
    {
        [PieceType.King] = BitPiece.King,
        [PieceType.Queen] = BitPiece.Queen,
        [PieceType.Pawn] = BitPiece.Pawn,
        [PieceType.Rook] = BitPiece.Rook,
        [PieceType.Bishop] = BitPiece.Bishop,
        [PieceType.Horsey] = BitPiece.Horsey,
        [PieceType.Knook] = BitPiece.Knook,
        [PieceType.Antiqueen] = BitPiece.Antiqueen,
        [PieceType.UnderagePawn] = BitPiece.UnderagePawn,
        [PieceType.SterilePawn] = BitPiece.SterilePawn,
        [PieceType.Checker] = BitPiece.Checker,
    };

    public static readonly IReadOnlyDictionary<PieceType, NeutralBitPiece> Neutral = new Dictionary<
        PieceType,
        NeutralBitPiece
    >
    {
        [PieceType.TraitorRook] = NeutralBitPiece.TraitorRook,
    };
}
