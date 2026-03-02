using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PlayerArchiveFaker : Faker<PlayerArchive>
{
    public PlayerArchiveFaker(GameColor color)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.UserId, f => (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.Color, color);
    }
}
