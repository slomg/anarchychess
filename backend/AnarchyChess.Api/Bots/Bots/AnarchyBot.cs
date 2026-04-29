using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class AnarchyBot(IBotService botService) : IBot
{
    public static readonly UserId BotId = "bot:anarchybot";
    private const int Depth = 8;

    public BotType Type => BotType.AnarchyBot;

    private readonly IBotService _botService = botService;

    public GamePlayer CreateBotPlayer(GameColor color) =>
        new(
            UserId: BotId,
            Color: color,
            UserName: "Anarchy Bot",
            CountryCode: "XX",
            Rating: 161660
        );

    public Task<ErrorOr<MoveEvaluation>> FindMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        CancellationToken token = default
    ) => _botService.FindBestMoveAsync(board, depth: Depth, token);
}
