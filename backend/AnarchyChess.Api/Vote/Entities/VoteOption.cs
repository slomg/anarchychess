using AnarchyChess.Api.Vote.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Vote.Entities;

[PrimaryKey(nameof(Key))]
public class VoteOption
{
    public required VoteOptionKey Key { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }
}
