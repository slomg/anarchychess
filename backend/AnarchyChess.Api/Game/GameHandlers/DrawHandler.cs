using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.GameHandlers;

public interface IDrawHandler
{
    Task<ErrorOr<Success>> HandleDeclineDrawAsync(
        GamePlayer player,
        GameToken gameToken,
        GameData game
    );
    Task<ErrorOr<GameEndStatus?>> HandleDrawRequestAsync(
        GamePlayer player,
        GameToken gameToken,
        GameData game
    );
}

public class DrawHandler(
    IOptions<AppSettings> settings,
    IGameResultDescriber resultDescriber,
    IGameNotifier notifier
) : IDrawHandler
{
    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IGameResultDescriber _resultDescriber = resultDescriber;
    private readonly IGameNotifier _notifier = notifier;

    public async Task<ErrorOr<GameEndStatus?>> HandleDrawRequestAsync(
        GamePlayer player,
        GameToken gameToken,
        GameData game
    )
    {
        if (game.DrawRequest.HasPendingRequest(player.Color))
        {
            return _resultDescriber.DrawByAgreement();
        }

        var requestResult = game.DrawRequest.RequestDraw(player.Color);
        if (requestResult.IsError)
            return requestResult.Errors;

        await _notifier.NotifyDrawStateChangeAsync(
            gameToken,
            game.DrawRequest.GetState(),
            game.NotifierState
        );
        return (GameEndStatus?)null;
    }

    public async Task<ErrorOr<Success>> HandleDeclineDrawAsync(
        GamePlayer player,
        GameToken gameToken,
        GameData game
    )
    {
        if (!game.DrawRequest.TryDeclineDraw(player.Color, _settings.DrawCooldown))
            return GameErrors.DrawNotRequested;

        await _notifier.NotifyDrawStateChangeAsync(
            gameToken,
            game.DrawRequest.GetState(),
            game.NotifierState
        );
        return Result.Success;
    }
}
