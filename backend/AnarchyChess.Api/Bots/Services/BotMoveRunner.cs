using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Errors;
using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.Shared.Services;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotMoveRunner
{
    void RunMove(IReadOnlyChessBoard board, int lastEval, GameToken gameToken, IBot bot);
}

public class BotMoveRunner(
    ILogger<BotMoveRunner> logger,
    IGrainFactory grains,
    IDelayProvider delayProvider
) : IBotMoveRunner
{
    private readonly ILogger<BotMoveRunner> _logger = logger;
    private readonly IGrainFactory _grains = grains;
    private readonly IDelayProvider _delayProvider = delayProvider;

    public const int MinBotMoveDelayMs = 1000;

    public void RunMove(IReadOnlyChessBoard board, int lastEval, GameToken gameToken, IBot bot)
    {
        _ = ExecuteMoveAsync(board, lastEval, gameToken, bot);
    }

    private async Task ExecuteMoveAsync(
        IReadOnlyChessBoard board,
        int lastEval,
        GameToken gameToken,
        IBot bot
    )
    {
        var grain = _grains.GetGrain<IBotGrain>(gameToken);
        try
        {
            var delayTask = _delayProvider.DelayAsync(MinBotMoveDelayMs);
            var botMoveResultTask = bot.FindMoveAsync(board, lastEval);
            await Task.WhenAll(botMoveResultTask, delayTask);

            var botMoveResult = await botMoveResultTask;
            await grain.PlayBotMoveAsync(botMoveResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing bot move in {GameToken}", gameToken);
            await grain.PlayBotMoveAsync(BotErrors.BotFailure);
        }
    }
}
