using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.GameLogic.Models;

[JsonConverter(typeof(StructStringJsonConverter<AlgebraicString>))]
[TypeConverter(typeof(StructStringTypeConverter<AlgebraicString>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct AlgebraicString(string Value)
{
    public static implicit operator string(AlgebraicString id) => id.Value;

    public static implicit operator AlgebraicString(string value) => new(value);

    public override string ToString() => Value;
}
