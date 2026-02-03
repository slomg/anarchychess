using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public struct BitMove
{
    public required byte From;
    public required byte To;
    public required BitPieceType Piece;

    public UInt128 Captures;
    public BitPieceType? PromotesTo;
    public ForcedMovePriority ForcedMovePriority;
    public BitMoveFlag Flags;
}
