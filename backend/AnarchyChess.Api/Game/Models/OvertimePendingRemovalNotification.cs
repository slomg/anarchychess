using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

public record OvertimePendingRemovalNotification(
    IReadOnlyList<byte> EncodedLegalMoves,
    AlgebraicPoint RemovePieceAt
);
