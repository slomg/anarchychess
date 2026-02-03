using AnarchyChess.Api.Profile.DTOs;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class ProfileEditFaker : Faker<ProfileEditRequest>
{
    public ProfileEditFaker()
    {
        StrictMode(true);
        RuleFor(x => x.About, f => f.Lorem.Sentence());
        RuleFor(x => x.CountryCode, "US");
    }
}
