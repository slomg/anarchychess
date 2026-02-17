using Orleans;
using ProtoBuf;

namespace AnarchyChess.EngineShared;

[ProtoContract]
[GenerateSerializer]
[Alias("AnarchyChess.EngineShared.Piece")]
public record Piece(PieceType Type, GameColor? Color, bool HasMoved = false);
