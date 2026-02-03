using Orleans;

namespace AnarchyChess.EngineShared;

[GenerateSerializer]
[Alias("AnarchyChess.EngineShared.Piece")]
public record Piece(PieceType Type, GameColor? Color, bool HasMoved = false);
