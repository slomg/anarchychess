using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Models;

public record PrevMoveState(
    byte From,
    byte To,
    BitPiece Piece,
    UInt128 CaptureMask,
    SpecialMoveType SpecialMoveType
);
