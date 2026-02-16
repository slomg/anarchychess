namespace AnarchyChess.Ai.Models;

public struct NullMoveUndoState
{
    public required bool PrevIsWhiteToMove;
    public required UInt128 PrevEnPassantSquaresMask;
    public required byte PrevEnPassantPawnSquare;
    public required UInt128 PrevLastCaptureMask;
}
