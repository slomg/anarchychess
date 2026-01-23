using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.OvertimePositionSnapshot")]
public record OvertimePositionSnapshot(
    IReadOnlyCollection<MovePath> LegalMoves,
    AlgebraicPoint RemovedPiece
);
