using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public interface IBot
{
    BotType Type { get; }
    Task<ErrorOr<MoveEvaluation>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    );
    GamePlayer CreateBotPlayer(GameColor color);
}
