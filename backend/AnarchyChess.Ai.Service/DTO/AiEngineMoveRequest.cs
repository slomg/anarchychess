using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Service.DTO;

public record AiEngineMoveRequest(
    Dictionary<AlgebraicPoint, Piece> Pieces,
    bool IsWhiteToMove,
    PrevMoveStateDto? PrevMoveState,
    int Depth
);
