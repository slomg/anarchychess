using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.Models;

public record MoveStun(AlgebraicPoint Position, Piece Piece, int StunForTurns);
