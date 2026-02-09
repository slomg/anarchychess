using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public struct BitMove
{
    public required byte From;
    public required byte To;
    public required BitPiece Piece;

    public UInt128 CapturesMask;
    public PieceType? PromotesTo;
    public ForcedMovePriority ForcedMovePriority;
    public SpecialMoveType SpecialMoveType;
}
