using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Models;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class VoteOptionFaker : Faker<VoteOption>
{
    public VoteOptionFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Key, f => (VoteOptionKey)f.Random.Guid().ToString());
        RuleFor(x => x.Name, f => f.Lorem.Sentence(wordCount: 3));
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
    }
}
