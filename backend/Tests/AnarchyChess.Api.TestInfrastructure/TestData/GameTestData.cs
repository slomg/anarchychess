using AnarchyChess.EngineShared;
using Bogus;

namespace AnarchyChess.Api.TestInfrastructure.TestData;

public static class GameTestData
{
    public const string InitialFen =
        "rhnbqkbcar/pppdppdppp/10/+9/10/10/9+/10/PPPDPPDPPP/RHNBQKBCAR";

    public static readonly string[] SanMoves =
    [
        "e4",
        "Nf3",
        "O-O",
        "Nc6",
        "e8=Q",
        "O-O-O",
        "d4",
        "Bc4",
        "a8=R",
        "exd6",
    ];

    public static byte[] RandomIdxs(Faker faker)
    {
        int length = faker.Random.Number(0, 5);
        byte[] idxs = new byte[length];
        for (int i = 0; i < length; i++)
        {
            idxs[i] = (byte)faker.Random.Number(0, 99);
        }
        return idxs;
    }

    public static AlgebraicPoint[] RandomPoints(Faker faker)
    {
        int length = faker.Random.Number(1, 5);
        AlgebraicPoint[] points = new AlgebraicPoint[length];
        for (int i = 0; i < length; i++)
        {
            points[i] = new AlgebraicPoint(
                X: faker.Random.Number(0, 9),
                Y: faker.Random.Number(0, 9)
            );
        }
        return points;
    }
}
