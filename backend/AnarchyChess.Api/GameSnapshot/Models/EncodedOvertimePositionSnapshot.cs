using System.ComponentModel;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("EncodedOvertimePosition")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.OvertimePositionSnapshot")]
public record EncodedOvertimePositionSnapshot(
    IReadOnlyCollection<MovePath> LegalMoves,
    AlgebraicPoint RemovedPiece
);
