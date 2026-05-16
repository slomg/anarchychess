using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Vote.Entities;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class UserVoteFaker : Faker<UserVote>
{
    public UserVoteFaker(UserId? userId = null, VoteOptionPair? pair = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.IpAddress, f => f.Internet.Ip());

        RuleFor(x => x.VotePair, f => pair ?? new VoteOptionPairFaker().Generate());
        RuleFor(x => x.VotePairId, (f, x) => pair?.Id ?? x.VotePair.Id);

        RuleFor(x => x.PickedOptionA, f => f.Random.Bool());
        RuleFor(x => x.VoteWeight, x => x.PickRandom(0.5f, 1));
        RuleFor(x => x.CreatedAt, DateTime.UtcNow);
    }
}
