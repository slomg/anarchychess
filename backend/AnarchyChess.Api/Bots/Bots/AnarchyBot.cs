using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.Profile.Models;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public class AnarchyBot(IBotService botService) : IBot
{
    private readonly IBotService _botService = botService;

    public static readonly UserId BotId = "bot:anarchybot";

    public UserId Id => BotId;

    public Task<ErrorOr<AiEngineMove>> FindMoveAsync(
        IReadOnlyChessBoard board,
        CancellationToken token = default
    ) => _botService.FindBestMoveAsync(board, token);
}
