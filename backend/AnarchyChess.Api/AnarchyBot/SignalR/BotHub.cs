using AnarchyChess.Api.AnarchyBot.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.SignalR;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace AnarchyChess.Api.AnarchyBot.SignalR;

public interface IBotHubClient : IAnarchyChessHubClient
{
    Task PlayerMadeMoveAsync(MoveSnapshot move, int plyNumber, bool didMoveEndGame);
    Task BotMadeMoveAsync(MoveSnapshot move, int plyNumber, CompressedMoves compressedLegalMoves);
    Task GameEndedAsync(GameResultData result);
}

[Authorize(AuthPolicies.ActiveSession)]
public class BotHub(IGrainFactory grains) : AnarchyChessHub<IBotHubClient>
{
    private readonly IGrainFactory _grains = grains;

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
}
