using System.ComponentModel;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Vote.Entities;

namespace AnarchyChess.Api.Vote.DTOs;

[DisplayName("PendingUserVote")]
[method: JsonConstructor]
public record PendingUserVoteDto(VoteOptionDto OptionA, VoteOptionDto OptionB)
{
    public PendingUserVoteDto(PendingUserVote pending)
        : this(OptionA: new(pending.VotePair.OptionA), OptionB: new(pending.VotePair.OptionB)) { }
}
