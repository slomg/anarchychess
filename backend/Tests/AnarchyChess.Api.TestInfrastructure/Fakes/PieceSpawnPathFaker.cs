using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PieceSpawnPathFaker : RecordFaker<PieceSpawnPath>
{
    public PieceSpawnPathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Type, f => f.PickRandom<PieceType>());
        RuleFor(x => x.Color, f => f.PickRandom<GameColor>());
        RuleFor(x => x.PosIdx, f => (byte)f.Random.Number(0, 99));
    }
}
