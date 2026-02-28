using AnarchyChess.Api.AnarchyBot.SignalR;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.AnarchyBot.Services;

public interface IBotNotifier
{
    Task NotifyBotMadeMoveAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int plyNumber,
        CompressedMoves compressedLegalMoves,
        int evalForBot,
        bool didMoveEndGame
    );

    Task NotifyPlayerMadeMoveAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int plyNumber,
        bool didMoveEndGame
    );

    Task NotifyGameEndedAsync(GameToken gameToken, GameResultData result);
    Task JoinBotGroupAsync(
        GameToken GameToken,
        ConnectionId connectionId,
        CancellationToken token = default
    );
}

public class BotNotifier(IHubContext<BotHub, IBotHubClient> hub) : IBotNotifier
{
    private readonly IHubContext<BotHub, IBotHubClient> _hub = hub;

    public Task NotifyPlayerMadeMoveAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int plyNumber,
        bool didMoveEndGame
    ) => _hub.Clients.Group(gameToken).PlayerMadeMoveAsync(move, plyNumber, didMoveEndGame);

    public Task NotifyBotMadeMoveAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int plyNumber,
        CompressedMoves compressedLegalMoves,
        int evalForBot,
        bool didMoveEndGame
    ) =>
        _hub
            .Clients.Group(gameToken)
            .BotMadeMoveAsync(move, plyNumber, compressedLegalMoves, evalForBot, didMoveEndGame);

    public Task NotifyGameEndedAsync(GameToken gameToken, GameResultData result) =>
        _hub.Clients.Group(gameToken).GameEndedAsync(result);

    public async Task JoinBotGroupAsync(
        GameToken GameToken,
        ConnectionId connectionId,
        CancellationToken token = default
    ) => await _hub.Groups.AddToGroupAsync(connectionId, GameToken, token);
}
