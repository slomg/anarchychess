using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.MoveHistory")]
public class MoveHistory
{
    [Id(0)]
    private readonly List<MoveSnapshot> _moveHistory = [];

    public IReadOnlyList<MoveSnapshot> Moves => _moveHistory;

    public MoveSnapshot AddMove(GameColor nextPlayer, MoveResult moveResult, double timeLeft)
    {
        MoveSnapshot moveSnapshot = new(
            Path: moveResult.MovePath,
            Fen: moveResult.Fen.FullFen,
            NextSideToMove: nextPlayer,
            San: moveResult.San,
            timeLeft
        );
        _moveHistory.Add(moveSnapshot);
        return moveSnapshot;
    }

    public void CommitOvertimeRemoval(AlgebraicPoint removal, int boardWidth)
    {
        if (_moveHistory.Count == 0)
        {
            return;
        }

        var removalIdx = removal.AsIndex(boardWidth);

        var lastMove = _moveHistory[^1];
        byte[] newRemovals = [.. lastMove.Path.OvertimeRemovalIdxs ?? [], removalIdx];
        var updatedPath = lastMove.Path with { OvertimeRemovalIdxs = newRemovals };
        _moveHistory[^1] = lastMove with { Path = updatedPath };
    }
}
