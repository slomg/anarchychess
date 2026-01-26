using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

public record OvertimeRemovalNotification(
    CompressedMoves EncodedLegalMoves,
    AlgebraicPoint RemoveFrom
);
