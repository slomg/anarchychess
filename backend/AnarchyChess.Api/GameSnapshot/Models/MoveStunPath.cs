using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MoveStunPath")]
public record MoveStunPath(byte PosIdx, int StunForTurns)
{
    public static MoveStunPath FromMoveStun(MoveStun stun, int boardWidth) =>
        new(PosIdx: stun.Position.AsIdx(boardWidth), StunForTurns: stun.StunForTurns);
}
