using ProtoBuf;

namespace AnarchyChess.Ai.Models;

[ProtoContract]
public record LastMoveState(
    byte EnPassantPawnSquare,
    UInt128 EnPassantSquaresMask,
    UInt128 LastCaptureMask
);
