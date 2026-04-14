using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Service.DTO;

public record AiEngineMoveRequest(
    IReadOnlyDictionary<AlgebraicPoint, Piece> Pieces,
    bool IsWhiteToMove,
    IReadOnlyDictionary<AlgebraicPoint, int> StunnedPositions,
    PrevMoveState? PrevMoveState,
    int Depth
);
