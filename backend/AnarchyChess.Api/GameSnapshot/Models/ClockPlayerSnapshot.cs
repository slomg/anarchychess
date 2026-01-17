using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("ClockPlayer")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.ClockPlayerSnapshot")]
public record ClockPlayerSnapshot(
    double TimeLeftMs,
    double? TimeUntilAbandonMs,
    bool IsInGracePeriod
);
