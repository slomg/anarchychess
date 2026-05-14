namespace AnarchyChess.Api.Vote.Entities;

public class VoteOptionPair
{
    public int Id { get; set; }

    public required VoteOption OptionA { get; set; }
    public required VoteOption OptionB { get; set; }
}
