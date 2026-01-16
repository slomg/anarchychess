namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.ClockPlayerSnapshot")]
public record ClockPlayerSnapshot(
    double TimeLeftMs,
    double? TimeUntilAbandonMs,
    bool IsInGracePeriod
);
