using System.ComponentModel.DataAnnotations.Schema;
using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Vote.Entities;

[PrimaryKey(nameof(UserId), nameof(VotePairId))]
[Index(nameof(IpAddress))]
public class UserVote
{
    public required UserId UserId { get; set; }
    public required string IpAddress { get; set; }

    [ForeignKey(nameof(VoteOptionPair))]
    public required int VotePairId { get; set; }
    public required VoteOptionPair VotePair { get; set; }

    public required bool PickedOptionA { get; set; }

    public required float VoteWeight { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
