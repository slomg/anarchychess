namespace EntropyBot;

struct BitMove
{
    public byte From;
    public byte To;
    public BitPiece Piece;

    public UInt128 Captures;
    public BitPiece? PromotesTo;
    public BitMovePriority ForcedMovePriority;
    public BitMoveFlag Flags;
}
