using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Vote.Entities;

namespace AnarchyChess.Api.Vote.DTOs;

[DisplayName("VoteOption")]
[method: JsonConstructor]
public record VoteOptionDto(int OptionId, string Name, string Description)
{
    public VoteOptionDto(VoteOption option)
        : this(OptionId: option.Id, Name: option.Name, Description: option.Description) { }
}
