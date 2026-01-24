using System.ComponentModel;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("EncodedPendingOvertimeRemoval")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.EncodedPendingOvertimeRemovalSnapshot")]
public record EncodedPendingOvertimeRemovalSnapshot(
    IReadOnlyCollection<MovePath> LegalMoves,
    AlgebraicPoint RemovedPiece
);
