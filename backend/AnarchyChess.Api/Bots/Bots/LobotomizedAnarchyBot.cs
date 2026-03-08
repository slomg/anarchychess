using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class LobotomizedAnarchyBot(IBotService botService) : IBot
{
    public static readonly UserId BotId = "bot:lobotomized-anarchybot";

    public BotType Type => BotType.LobotomizedAnarchyBot;

    private readonly IBotService botService = botService;

    public Task<ErrorOr<AiEngineMove>> FindMoveAsync(
        IReadOnlyChessBoard board,
        CancellationToken token = default
    )
    {
        throw new NotImplementedException();
    }

    public GamePlayer CreateBotPlayer(GameColor color) =>
        new(
            UserId: BotId,
            Color: color,
            UserName: "Lobotomized Anarchy Bot",
            CountryCode: "FR",
            Rating: -161660
        );
}
