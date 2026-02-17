using AnarchyChess.EngineShared;
using ProtoBuf;

namespace AnarchyChess.Ai.Service.DTO;

[ProtoContract]
public record AiEngineMoveReply(
    AlgebraicPoint From,
    AlgebraicPoint To,
    IReadOnlyCollection<AlgebraicPoint> Captures,
    PieceType? PromotesTo
);
