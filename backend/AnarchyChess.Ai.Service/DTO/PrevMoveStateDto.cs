using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Service.DTO;

public record PrevMoveStateDto(
    AlgebraicPoint From,
    AlgebraicPoint To,
    Piece Piece,
    IReadOnlyCollection<AlgebraicPoint>? Captures
);
