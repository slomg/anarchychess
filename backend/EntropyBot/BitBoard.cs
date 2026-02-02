namespace AnarchyBot;

public struct BitBoard
{
    public UInt128 WhiteKings;
    public UInt128 WhiteQueens;
    public UInt128 WhitePawns;
    public UInt128 WhiteRooks;
    public UInt128 WhiteBishops;
    public UInt128 WhiteHorsies;
    public UInt128 WhiteKnooks;
    public UInt128 WhiteAntiqueens;
    public UInt128 WhiteUnderagePawns;
    public UInt128 WhiteSterilePawns;
    public UInt128 WhiteCheckers;

    public UInt128 BlackKings;
    public UInt128 BlackQueens;
    public UInt128 BlackPawns;
    public UInt128 BlackRooks;
    public UInt128 BlackBishops;
    public UInt128 BlackHorsies;
    public UInt128 BlackKnooks;
    public UInt128 BlackAntiqueens;
    public UInt128 BlackUnderagePawns;
    public UInt128 BlackSterilePawns;
    public UInt128 BlackCheckers;

    public UInt128 WhitePieces;
    public UInt128 BlackPieces;
    public UInt128 Empty;

    public UInt128 TraitorRooks;

    public readonly UInt128 BitboardFor(BitPiece pieceType) =>
        pieceType switch
        {
            BitPiece.WhiteKing => WhiteKings,
            BitPiece.WhiteQueen => WhiteQueens,
            BitPiece.WhitePawn => WhitePawns,
            BitPiece.WhiteRook => WhiteRooks,
            BitPiece.WhiteBishop => WhiteBishops,
            BitPiece.WhiteHorsey => WhiteHorsies,
            BitPiece.WhiteKnook => WhiteKnooks,
            BitPiece.WhiteAntiqueen => WhiteAntiqueens,
            BitPiece.WhiteUnderagePawn => WhiteUnderagePawns,
            BitPiece.WhiteSterilePawn => WhiteSterilePawns,
            BitPiece.WhiteChecker => WhiteCheckers,

            BitPiece.BlackKing => BlackKings,
            BitPiece.BlackQueen => BlackQueens,
            BitPiece.BlackPawn => BlackPawns,
            BitPiece.BlackRook => BlackRooks,
            BitPiece.BlackBishop => BlackBishops,
            BitPiece.BlackHorsey => BlackHorsies,
            BitPiece.BlackKnook => BlackKnooks,
            BitPiece.BlackAntiqueen => BlackAntiqueens,
            BitPiece.BlackUnderagePawn => BlackUnderagePawns,
            BitPiece.BlackSterilePawn => BlackSterilePawns,
            BitPiece.BlackChecker => BlackCheckers,

            BitPiece.TraitorRook => TraitorRooks,

            _ => throw new ArgumentOutOfRangeException(
                nameof(pieceType),
                pieceType,
                "Invalid piece"
            ),
        };
}
