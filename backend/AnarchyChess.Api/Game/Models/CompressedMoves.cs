using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.CompressedMoves")]
[JsonConverter(typeof(StructStringJsonConverter<CompressedMoves>))]
[TypeConverter(typeof(StructStringTypeConverter<CompressedMoves>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct CompressedMoves(string Value)
{
    public static implicit operator string(CompressedMoves moves) => moves.Value;

    public static implicit operator CompressedMoves(string value) => new(value);

    public override string ToString() => Value;
}
