using AnarchyChess.EngineShared;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PieceFaker : RecordFaker<Piece>
{
    public PieceFaker(GameColor? color, PieceType? piece = null, bool? hasMoved = null)
    {
        UseSeed(Faker.GlobalUniqueIndex++);

        StrictMode(true);
        RuleFor(x => x.Type, f => piece ?? f.PickRandom<PieceType>());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.HasMoved, f => hasMoved ?? f.Random.Bool());
    }
}
