using AnarchyChess.EngineShared;
using ProtoBuf;

namespace AnarchyChess.Ai.Models;

[ProtoContract]
public struct BitMove
{
    [ProtoMember(1)]
    public required byte From;

    [ProtoMember(2)]
    public required byte To;

    [ProtoMember(3)]
    public required BitPiece Piece;

    [ProtoMember(4)]
    public UInt128 CapturesMask;

    [ProtoMember(5)]
    public PieceType? PromotesTo;

    [ProtoMember(6)]
    public ForcedMovePriority ForcedMovePriority;

    [ProtoMember(7)]
    public SpecialMoveType SpecialMoveType;
}
