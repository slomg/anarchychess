namespace EntropyBot;

public struct Board
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

    public UInt128 TraitorRooks;

    public UInt128 WhitePieces;
    public UInt128 BlackPieces;

    public bool WhiteToMove;
    public int HalfMoveClock;
}
