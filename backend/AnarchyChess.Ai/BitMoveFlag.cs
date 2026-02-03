namespace AnarchyChess.Ai;

[Flags]
public enum BitMoveFlag : byte
{
    None = 0,
    KingSideCastling = 1 << 0,
    QueenSideCastling = 1 << 1,
    VerticalCastling = 1 << 2,
    EnPassant = 1 << 3,
    BetaDecay = 1 << 4,
}
