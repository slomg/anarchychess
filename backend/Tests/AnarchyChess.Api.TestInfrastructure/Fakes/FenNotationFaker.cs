using AnarchyChess.Api.Game.Services;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class FenNotationFaker : RecordFaker<FenNotation>
{
    public FenNotationFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Position, f => f.Random.AlphaNumeric(10));
        RuleFor(x => x.FullFen, f => f.Random.AlphaNumeric(10));
    }
}
