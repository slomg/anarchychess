namespace EntropyBot;

struct BitMove
{
    public int From;
    public int To;
    public BitPiece Piece;

    public UInt128 Captures;
    public BitPiece? PromotesTo;
    public BitMoveFlag Flags;
}
