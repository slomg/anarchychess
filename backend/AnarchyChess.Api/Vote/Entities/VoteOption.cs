namespace AnarchyChess.Api.Vote.Entities;

public class VoteOption
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }
}
