using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("EncodedPlayerOvertime")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.PlayerOvertimeSnapshot")]
public record EncodedPlayerOvertimeSnapshot(
    double SecondRemainder,
    IReadOnlyList<EncodedOvertimePositionSnapshot> PendingRemoval
);
