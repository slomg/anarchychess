using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Service.DTO;

public record AiEngineMoveReply(
    AlgebraicPoint From,
    AlgebraicPoint To,
    IReadOnlyCollection<AlgebraicPoint>? Captures,
    PieceType? PromotesTo,
    int EvalForBot
);
