using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class ClockSnapshotFaker : RecordFaker<ClockSnapshot>
{
    public ClockSnapshotFaker(double? whiteTimeLeftMs = null, double? blackTimeLeftMs = null)
    {
        StrictMode(true);
        RuleFor(x => x.WhiteClock, f => new ClockPlayerSnapshotFaker(whiteTimeLeftMs));
        RuleFor(x => x.BlackClock, f => new ClockPlayerSnapshotFaker(blackTimeLeftMs));
        RuleFor(x => x.LastUpdated, f => f.Random.Double(1000000, 10000000));
        RuleFor(x => x.ServerTime, f => f.Random.Double(1000000, 10000000));
        RuleFor(x => x.IsFrozen, false);
    }
}
