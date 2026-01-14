using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("Clocks")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.ClockSnapshot")]
public record ClockSnapshot(
    [property: Id(0)] double WhiteClock,
    [property: Id(1)] double BlackClock,
    [property: Id(2)] double LastUpdated,
    [property: Id(3)] bool IsFrozen,
    [property: Id(4)] double ServerTime
);
