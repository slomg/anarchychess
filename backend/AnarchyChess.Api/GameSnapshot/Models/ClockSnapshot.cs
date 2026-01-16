using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("Clocks")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.ClockSnapshot")]
public record ClockSnapshot(
    ClockPlayerSnapshot WhiteClock,
    ClockPlayerSnapshot BlackClock,
    double LastUpdated,
    bool IsFrozen,
    double ServerTime
);
