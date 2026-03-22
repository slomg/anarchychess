using Orleans;

namespace AnarchyChess.EngineShared;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.Piece")]
public record Piece(
    PieceType Type,
    GameColor? Color,
    bool HasMoved = false,
    int StunnedForTurns = 0
);
