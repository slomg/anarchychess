using AnarchyChess.Api.AnarchyBot.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;

namespace AnarchyChess.Api.AnarchyBot.Services;

public interface IBotMoveRunner
{
    void RunMove(IReadOnlyChessBoard board, GameToken gameToken);
}

public class BotMoveRunner(IBotService botService, IGrainFactory grains) : IBotMoveRunner
{
    private readonly IBotService _botService = botService;
    private readonly IGrainFactory _grains = grains;

    public void RunMove(IReadOnlyChessBoard board, GameToken gameToken)
    {
        Task.Run(() => ExecuteMoveAsync(board, gameToken));
    }

    private async Task ExecuteMoveAsync(IReadOnlyChessBoard board, GameToken gameToken)
    {
        var botMoveResult = await _botService.FindBestMoveAsync(board);
        var grain = _grains.GetGrain<IBotGrain>(gameToken);
        await grain.PlayBotMoveAsync(botMoveResult);
    }
}
