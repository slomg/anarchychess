using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MoveStunPathFaker : RecordFaker<MoveStunPath>
{
    public MoveStunPathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.PosIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.StunForTurns, f => f.Random.Number(0, 5));
    }
}
