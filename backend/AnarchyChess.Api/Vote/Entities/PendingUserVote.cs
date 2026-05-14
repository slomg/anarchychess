using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Vote.Entities;

[PrimaryKey(nameof(UserId))]
public class PendingUserVote
{
    public required UserId UserId { get; set; }
    public required VoteOptionPair VotePair { get; set; }
}
