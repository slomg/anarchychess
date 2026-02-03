using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Constants;

public static class BitPieceMap
{
    public static readonly IReadOnlyDictionary<PieceType, BitPieceType> Colored = new Dictionary<
        PieceType,
        BitPieceType
    >
    {
        [PieceType.King] = BitPieceType.King,
        [PieceType.Queen] = BitPieceType.Queen,
        [PieceType.Pawn] = BitPieceType.Pawn,
        [PieceType.Rook] = BitPieceType.Rook,
        [PieceType.Bishop] = BitPieceType.Bishop,
        [PieceType.Horsey] = BitPieceType.Horsey,
        [PieceType.Knook] = BitPieceType.Knook,
        [PieceType.Antiqueen] = BitPieceType.Antiqueen,
        [PieceType.UnderagePawn] = BitPieceType.UnderagePawn,
        [PieceType.SterilePawn] = BitPieceType.SterilePawn,
        [PieceType.Checker] = BitPieceType.Checker,
    };

    public static readonly IReadOnlyDictionary<PieceType, NeutralBitPieceType> Neutral =
        new Dictionary<PieceType, NeutralBitPieceType>
        {
            [PieceType.TraitorRook] = NeutralBitPieceType.TraitorRook,
        };
}
