using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("PlayerOvertime")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.PlayerOvertimeSnapshot")]
public record PlayerOvertimeSnapshot(
    double SecondRemainder,
    IReadOnlyList<EncodedOvertimePositionSnapshot> PendingRemoval
);
