using AnarchyChess.Api.AnarchyBot.SignalR;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.AnarchyBot.Services;

public interface IAnarchyBotNotifier
{
    Task NotifyBotMadeMoveAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int plyNumber,
        CompressedMoves compressedLegalMoves
    );

    Task NotifyPlayerMadeMoveAsync(
        GameToken gameToken,
        MoveSnapshot move,
        int plyNumber,
        bool didMoveEndGame
    );
}

public class AnarchyBotNotifier(IHubContext<AnarchyBotHub, IAnarchyBotHubClient> hub)
    : IAnarchyBotNotifier
{
    private readonly IHubContext<AnarchyBotHub, IAnarchyBotHubClient> _hub = hub;

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
        CompressedMoves compressedLegalMoves
    ) => _hub.Clients.Group(gameToken).BotMadeMoveAsync(move, plyNumber, compressedLegalMoves);
}
