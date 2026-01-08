using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class OpenSeekFaker : RecordFaker<OpenSeek>
{
    public OpenSeekFaker()
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.UserName, f => f.Person.UserName);
        RuleFor(x => x.Pool, f => new PoolKeyFaker().Generate());
        RuleFor(x => x.Rating, f => f.Random.Number(1000, 3000));
    }
}
