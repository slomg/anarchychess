namespace AnarchyChess.Ai.Models;

public struct NullMoveUndoState
{
    public required bool IsWhiteToMove;
    public required UInt128 EnPassantSquaresMask;
    public required byte EnPassantPawnSquare;
    public required bool CanSpawnOmnipotentPawn;
    public required ulong ZobristKey;
    public required UInt128 StunnedPieces;
}
