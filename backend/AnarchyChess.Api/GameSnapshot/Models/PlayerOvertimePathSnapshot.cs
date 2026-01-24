using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("PlayerOvertimePath")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.PlayerOvertimePathSnapshot")]
public record PlayerOvertimePathSnapshot(
    double SecondRemainderMs,
    IReadOnlyList<PendingOvertimeRemovalPathSnapshot> PendingRemoval
);
