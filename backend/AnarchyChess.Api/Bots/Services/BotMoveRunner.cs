using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotMoveRunner
{
    void RunMove(IReadOnlyChessBoard board, GameToken gameToken, IBot bot);
}

public class BotMoveRunner(IGrainFactory grains) : IBotMoveRunner
{
    private readonly IGrainFactory _grains = grains;

    public void RunMove(IReadOnlyChessBoard board, GameToken gameToken, IBot bot)
    {
        Task.Run(() => ExecuteMoveAsync(board, gameToken, bot));
    }

    private async Task ExecuteMoveAsync(IReadOnlyChessBoard board, GameToken gameToken, IBot bot)
    {
        var botMoveResult = await bot.FindMoveAsync(board);
        var grain = _grains.GetGrain<IBotGrain>(gameToken);
        await grain.PlayBotMoveAsync(botMoveResult);
    }
}
