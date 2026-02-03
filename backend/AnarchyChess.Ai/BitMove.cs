using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public struct BitMove
{
    public required byte From;
    public required byte To;
    public required PieceType Piece;

    public UInt128 Captures;
    public PieceType? PromotesTo;
    public ForcedMovePriority ForcedMovePriority;
    public SpecialMoveType SpecialMoveType;
}
