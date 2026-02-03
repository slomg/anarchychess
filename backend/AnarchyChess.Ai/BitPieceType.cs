namespace AnarchyChess.Ai;

public enum BitPieceType
{
    WHITE_START_MARKER,
    WhiteKing,
    WhiteQueen,
    WhitePawn,
    WhiteRook,
    WhiteBishop,
    WhiteHorsey,
    WhiteKnook,
    WhiteAntiqueen,
    WhiteUnderagePawn,
    WhiteSterilePawn,
    WhiteChecker,
    WHITE_END_MARKER,

    BLACK_START_MARKER,
    BlackKing,
    BlackQueen,
    BlackPawn,
    BlackRook,
    BlackBishop,
    BlackHorsey,
    BlackKnook,
    BlackAntiqueen,
    BlackUnderagePawn,
    BlackSterilePawn,
    BlackChecker,
    BLACK_END_MARKER,

    NEUTRAL_START_MARKER,
    TraitorRook,
    NEUTRAL_END_MARKER,
}
