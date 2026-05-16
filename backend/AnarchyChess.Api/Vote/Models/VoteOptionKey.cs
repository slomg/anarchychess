using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Infrastructure;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace AnarchyChess.Api.Vote.Models;

[JsonConverter(typeof(StructStringJsonConverter<VoteOptionKey>))]
[TypeConverter(typeof(StructStringTypeConverter<VoteOptionKey>))]
[JsonSchema(JsonObjectType.String)]
public readonly record struct VoteOptionKey(string Value)
{
    public static implicit operator string(VoteOptionKey key) => key.Value;

    public static implicit operator VoteOptionKey(string value) => new(value);

    public override string ToString() => Value;
}
