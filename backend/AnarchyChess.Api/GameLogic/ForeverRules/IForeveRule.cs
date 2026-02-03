using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.ForeverRules;

public interface IForeveRule
{
    IEnumerable<Move> GetBehaviours(IReadOnlyChessBoard board, GameColor movingPlayer);
}
