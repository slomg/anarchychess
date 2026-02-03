namespace AnarchyChess.Ai;

public struct BitMove
{
    public byte From;
    public byte To;
    public BitPieceType Piece;

    public UInt128 Captures;
    public BitPieceType? PromotesTo;
    public BitMovePriority ForcedMovePriority;
    public BitMoveFlag Flags;
}
