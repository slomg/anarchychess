using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.MoveStun")]
public record MoveStun(AlgebraicPoint Position, Piece Piece, int StunForTurns);
