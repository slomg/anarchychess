using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class ClockPlayerSnapshotFaker : RecordFaker<ClockPlayerSnapshot>
{
    public ClockPlayerSnapshotFaker(double? timeLeftMs = null)
    {
        StrictMode(true);
        RuleFor(x => x.TimeLeftMs, f => timeLeftMs ?? f.Random.Double(1000, 100000));
        RuleFor(x => x.TimeUntilAbandonMs, (double?)null);
        RuleFor(x => x.IsInGracePeriod, false);
    }
}
