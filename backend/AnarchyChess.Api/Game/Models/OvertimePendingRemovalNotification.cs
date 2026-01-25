using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

public record OvertimePendingRemovalNotification(
    CompressedMoves EncodedLegalMoves,
    AlgebraicPoint RemoveFrom,
    long RemoveAtTimestamp
);
