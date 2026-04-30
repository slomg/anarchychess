using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Quests.Entities;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class UserQuestPointsFaker : Faker<UserQuestPoints>
{
    public UserQuestPointsFaker(AuthedUser? user = null)
    {
        StrictMode(true);
        RuleFor(x => x.User, f => user ?? new AuthedUserFaker().Generate());
        RuleFor(x => x.UserId, (f, x) => x.User.Id);
        RuleFor(x => x.TotalPoints, f => (f.IndexFaker + 1) * 2);
        RuleFor(x => x.MonthlyPoints, f => f.IndexFaker + 1);
    }
}
