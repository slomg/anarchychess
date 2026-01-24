using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

public record OvertimePositionNotification(
    IReadOnlyList<byte> EncodedLegalMoves,
    AlgebraicPoint RemovedPiece
);
