using AnarchyChess.Api.Vote.Entities;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class VoteOptionPairFaker : Faker<VoteOptionPair>
{
    public VoteOptionPairFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, f => f.IndexFaker);

        RuleFor(x => x.OptionA, f => new VoteOptionFaker().Generate());
        RuleFor(x => x.OptionAKey, (f, x) => x.OptionA.Key);

        RuleFor(x => x.OptionB, f => new VoteOptionFaker().Generate());
        RuleFor(x => x.OptionBKey, (f, x) => x.OptionB.Key);
    }
}
