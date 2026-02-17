using AnarchyChess.EngineShared;
using ProtoBuf;

namespace AnarchyChess.Ai.Service.DTO;

[ProtoContract]
public record PrevMoveStateDto(
    AlgebraicPoint From,
    AlgebraicPoint To,
    Piece Piece,
    IReadOnlyCollection<AlgebraicPoint> LastCaptures
);
