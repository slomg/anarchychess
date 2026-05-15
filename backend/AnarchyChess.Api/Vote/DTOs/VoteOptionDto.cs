using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Models;

namespace AnarchyChess.Api.Vote.DTOs;

[DisplayName("VoteOption")]
[method: JsonConstructor]
public record VoteOptionDto(VoteOptionKey OptionKey, string Name, string Description)
{
    public VoteOptionDto(VoteOption option)
        : this(OptionKey: option.Key, Name: option.Name, Description: option.Description) { }
}
