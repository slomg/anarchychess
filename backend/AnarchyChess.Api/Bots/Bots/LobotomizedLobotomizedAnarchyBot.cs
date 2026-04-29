using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class LobotomizedLobotomizedAnarchyBot : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-lobotomized-anarchybot";

    public BotType Type => BotType.LobotomizedLobotomizedAnarchyBot;

    public GamePlayer CreateBotPlayer(GameColor color)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<MoveEvaluation>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    )
    {
        throw new NotImplementedException();
    }
}
