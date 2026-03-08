using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.Profile.Models;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class LobotomizedAnarchyBot : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-anarchybot";

    public UserId Id => BotId;

    public Task<ErrorOr<AiEngineMove>> FindMoveAsync(
        IReadOnlyChessBoard board,
        CancellationToken token = default
    )
    {
        throw new NotImplementedException();
    }
}
