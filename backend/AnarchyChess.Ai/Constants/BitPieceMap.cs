using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Constants;

public static class BitPieceMap
{
    public static readonly IReadOnlyDictionary<
        (PieceType, GameColor?),
        BitPieceType
    > PieceTypeToBitPieceType = new Dictionary<(PieceType, GameColor?), BitPieceType>
    {
        [(PieceType.King, GameColor.White)] = BitPieceType.WhiteKing,
        [(PieceType.Queen, GameColor.White)] = BitPieceType.WhiteQueen,
        [(PieceType.Pawn, GameColor.White)] = BitPieceType.WhitePawn,
        [(PieceType.Rook, GameColor.White)] = BitPieceType.WhiteRook,
        [(PieceType.Bishop, GameColor.White)] = BitPieceType.WhiteBishop,
        [(PieceType.Horsey, GameColor.White)] = BitPieceType.WhiteHorsey,
        [(PieceType.Knook, GameColor.White)] = BitPieceType.WhiteKnook,
        [(PieceType.Antiqueen, GameColor.White)] = BitPieceType.WhiteAntiqueen,
        [(PieceType.UnderagePawn, GameColor.White)] = BitPieceType.WhiteUnderagePawn,
        [(PieceType.SterilePawn, GameColor.White)] = BitPieceType.WhiteSterilePawn,
        [(PieceType.Checker, GameColor.White)] = BitPieceType.WhiteChecker,

        [(PieceType.King, GameColor.Black)] = BitPieceType.BlackKing,
        [(PieceType.Queen, GameColor.Black)] = BitPieceType.BlackQueen,
        [(PieceType.Pawn, GameColor.Black)] = BitPieceType.BlackPawn,
        [(PieceType.Rook, GameColor.Black)] = BitPieceType.BlackRook,
        [(PieceType.Bishop, GameColor.Black)] = BitPieceType.BlackBishop,
        [(PieceType.Horsey, GameColor.Black)] = BitPieceType.BlackHorsey,
        [(PieceType.Knook, GameColor.Black)] = BitPieceType.BlackKnook,
        [(PieceType.Antiqueen, GameColor.Black)] = BitPieceType.BlackAntiqueen,
        [(PieceType.UnderagePawn, GameColor.Black)] = BitPieceType.BlackUnderagePawn,
        [(PieceType.SterilePawn, GameColor.Black)] = BitPieceType.BlackSterilePawn,
        [(PieceType.Checker, GameColor.Black)] = BitPieceType.BlackChecker,

        [(PieceType.TraitorRook, null)] = BitPieceType.TraitorRook,
    };

    public static BitPieceType FromPiece(PieceType pieceType, GameColor? color) =>
        PieceTypeToBitPieceType[(pieceType, color)];
}
