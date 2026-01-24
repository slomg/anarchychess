using System.ComponentModel;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("PendingOvertimeRemovalPath")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.PendingOvertimeRemovalPathSnapshot")]
public record PendingOvertimeRemovalPathSnapshot(
    IReadOnlyCollection<MovePath> LegalMoves,
    AlgebraicPoint RemovedPiece
);
