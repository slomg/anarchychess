using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Service.DTO;

public record AiEngineMoveRequest(
    Dictionary<AlgebraicPoint, Piece> Pieces,
    bool IsWhiteToMove,
    PrevMoveState? PrevMoveState,
    int Depth,
    IReadOnlyCollection<AlgebraicPoint>? StunnedPositions = null
);
