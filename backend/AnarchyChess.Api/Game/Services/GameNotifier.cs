using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.SignalR;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Game.Services;

public interface IGameNotifier
{
    Task SyncRevisionAsync(ConnectionId connectionId, GameNotifierState state);
    Task JoinGameGroupAsync(GameToken gameToken, UserId userId, ConnectionId connectionId);
    Task NotifyDrawStateChangeAsync(
        GameToken gameToken,
        DrawState drawState,
        GameNotifierState state
    );
    Task NotifyGameEndedAsync(
        GameToken gameToken,
        GameResultData result,
        ClockSnapshot finalClocks,
        GameNotifierState state
    );
    Task NotifyMoveMadeAsync(MoveNotification notification, GameNotifierState state);
    Task NotifyOvertimeAsync(
        AlgebraicPoint removeFrom,
        CompressedMoves encodedLegalMoves,
        GameToken gameToken,
        GameNotifierState state
    );
}

public record MoveNotification(
    GameToken GameToken,
    MoveSnapshot Move,
    int PlyNumber,
    ClockSnapshot Clocks,
    UserId SideToMoveUserId,
    CompressedMoves EncodedLegalMoves,
    bool DidMoveEndGame
);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.GameNotifierState")]
public class GameNotifierState
{
    [Id(0)]
    public int Revision { get; set; }
}

public class GameNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    private static string UserGameGroup(GameToken gameToken, UserId userId) =>
        $"{gameToken}:{userId}";

    public Task SyncRevisionAsync(ConnectionId connectionId, GameNotifierState state) =>
        _hub.Clients.Client(connectionId).SyncRevisionAsync(state.Revision);

    public async Task NotifyMoveMadeAsync(MoveNotification notification, GameNotifierState state)
    {
        state.Revision++;
        await _hub
            .Clients.Group(notification.GameToken)
            .MoveMadeAsync(
                move: notification.Move,
                plyNumber: notification.PlyNumber,
                clock: notification.Clocks,
                didMoveEndGame: notification.DidMoveEndGame
            );
        await _hub
            .Clients.Group(UserGameGroup(notification.GameToken, notification.SideToMoveUserId))
            .OpponentMoveMadeAsync(
                move: notification.Move,
                plyNumber: notification.PlyNumber,
                encodedLegalMoves: notification.EncodedLegalMoves,
                clock: notification.Clocks
            );
    }

    public async Task NotifyOvertimeAsync(
        AlgebraicPoint removeFrom,
        CompressedMoves encodedLegalMoves,
        GameToken gameToken,
        GameNotifierState state
    )
    {
        state.Revision++;
        await _hub.Clients.Group(gameToken).ReceiveOvertimeAsync(removeFrom, encodedLegalMoves);
    }

    public Task NotifyDrawStateChangeAsync(
        GameToken gameToken,
        DrawState drawState,
        GameNotifierState state
    )
    {
        state.Revision++;
        return _hub.Clients.Group(gameToken).DrawStateChangeAsync(drawState);
    }

    public Task NotifyGameEndedAsync(
        GameToken gameToken,
        GameResultData result,
        ClockSnapshot finalClocks,
        GameNotifierState state
    )
    {
        state.Revision++;
        return _hub.Clients.Group(gameToken).GameEndedAsync(result, finalClocks);
    }

    public async Task JoinGameGroupAsync(
        GameToken gameToken,
        UserId userId,
        ConnectionId connectionId
    )
    {
        await _hub.Groups.AddToGroupAsync(connectionId, gameToken);
        await _hub.Groups.AddToGroupAsync(connectionId, UserGameGroup(gameToken, userId));
    }
}
