using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Vote.Entities;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PendingUserVoteFaker : Faker<PendingUserVote>
{
    public PendingUserVoteFaker(UserId? userId = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => userId ?? (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.VotePair, f => new VoteOptionPairFaker().Generate());
    }
}
