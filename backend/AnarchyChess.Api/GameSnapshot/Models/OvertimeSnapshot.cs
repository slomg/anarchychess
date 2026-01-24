using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("Overtime")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.OvertimeSnapshot")]
public record OvertimeSnapshot(
    EncodedPlayerOvertimeSnapshot? WhiteOvertime,
    EncodedPlayerOvertimeSnapshot? BlackOvertime
);
