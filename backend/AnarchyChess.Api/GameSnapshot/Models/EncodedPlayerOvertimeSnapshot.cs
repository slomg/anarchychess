using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("EncodedPlayerOvertime")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.EncodedPlayerOvertimeSnapshot")]
public record EncodedPlayerOvertimeSnapshot(
    double SecondRemainder,
    IReadOnlyList<EncodedPendingOvertimeRemovalSnapshot> PendingRemoval
);
