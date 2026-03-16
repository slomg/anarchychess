using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.SignalR;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Bots.SignalR;

public interface IBotHubClient : IAnarchyChessHubClient
{
    Task SyncPlyNumberAsync(int plyNumber);
    Task PlayerMadeMoveAsync(MoveSnapshot move, int plyNumber, bool didMoveEndGame);
    Task BotMadeMoveAsync(
        MoveSnapshot move,
        int plyNumber,
        CompressedMoves compressedLegalMoves,
        int evalForBot,
        bool didMoveEndGame
    );
    Task GameEndedAsync(GameResultData result);
}

[Authorize(AuthPolicies.ActiveSession)]
public class BotHub(IGrainFactory grains, IBotNotifier notifier) : AnarchyChessHub<IBotHubClient>
{
    private const string GameTokenQueryParam = "gameToken";

    private readonly IGrainFactory _grains = grains;
    private readonly IBotNotifier _notifier = notifier;

    public async Task MakeMoveAsync(GameToken gameToken, MoveKey key)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var result = await _grains.GetGrain<IBotGrain>(gameToken).PlayMoveAsync(userId, key);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public async Task ResignAsync(GameToken gameToken)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var result = await _grains.GetGrain<IBotGrain>(gameToken).ResignAsync(userId);
        if (result.IsError)
        {
            await HandleErrors(result.Errors);
            return;
        }
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        string? gameToken = Context.GetHttpContext()?.Request.Query[GameTokenQueryParam];
        if (string.IsNullOrWhiteSpace(gameToken))
        {
            return;
        }

        await _notifier.JoinBotGroupAsync(gameToken, Context.ConnectionId);

        var grain = _grains.GetGrain<IBotGrain>(gameToken);
        await grain.SyncPlyNumberAsync(Context.ConnectionId);
    }
}
