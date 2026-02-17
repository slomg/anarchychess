using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;
using ProtoBuf;

namespace AnarchyChess.Ai.Service.DTO;

[ProtoContract]
public record AiEngineMoveRequest(
    Dictionary<AlgebraicPoint, Piece> Pieces,
    bool IsWhiteToMove,
    LastMoveState? LastMoveState
);
