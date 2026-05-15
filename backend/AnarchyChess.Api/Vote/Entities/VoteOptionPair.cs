using System.ComponentModel.DataAnnotations.Schema;
using AnarchyChess.Api.Vote.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Vote.Entities;

[Index(nameof(OptionAKey), nameof(OptionBKey), IsUnique = true)]
public class VoteOptionPair
{
    public int Id { get; set; }

    [ForeignKey(nameof(VoteOption))]
    public required VoteOptionKey OptionAKey { get; set; }
    public required VoteOption OptionA { get; set; }

    [ForeignKey(nameof(VoteOption))]
    public required VoteOptionKey OptionBKey { get; set; }
    public required VoteOption OptionB { get; set; }
}
