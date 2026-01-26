using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.GameHandlers;

public interface IMoveHandler
{
    Task<ErrorOr<GameEndStatus?>> HandleMoveAsync(
        UserId moveMadeBy,
        MoveKey key,
        GameToken gameToken,
        GameData game,
        CancellationToken token = default
    );
}

public class MoveHandler(
    ILogger<MoveHandler> logger,
    IOptions<AppSettings> settings,
    IGameCore core,
    IGameClock clock,
    IGameNotifier notifier,
    IOvertime overtime
) : IMoveHandler
{
    private readonly ILogger<MoveHandler> _logger = logger;
    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IGameCore _core = core;
    private readonly IGameClock _clock = clock;
    private readonly IGameNotifier _notifier = notifier;
    private readonly IOvertime _overtime = overtime;

    public async Task<ErrorOr<GameEndStatus?>> HandleMoveAsync(
        UserId moveMadeBy,
        MoveKey key,
        GameToken gameToken,
        GameData game,
        CancellationToken token = default
    )
    {
        var currentPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        if (currentPlayer.UserId != moveMadeBy)
        {
            _logger.LogWarning(
                "User {UserId} attempted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                moveMadeBy,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var makeMoveResult = _core.MakeMove(key, game.Core);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;
        var moveResult = makeMoveResult.Value;

        var nextPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        var moveSnapshot = BuildAndStoreMove(
            movedBy: currentPlayer.Color,
            nextPlayer: nextPlayer.Color,
            moveResult,
            game
        );

        await _notifier.NotifyMoveMadeAsync(
            notification: new(
                GameToken: gameToken,
                Move: moveSnapshot,
                PlyNumber: game.MoveHistory.Moves.Count,
                Clocks: _clock.ToSnapshot(game.ClockState),
                SideToMoveUserId: nextPlayer.UserId,
                EncodedLegalMoves: _core.EncodeLegalMoves(game.Core),
                DidMoveEndGame: moveResult.EndStatus is not null
            ),
            game.NotifierState
        );

        await HandleDrawForMoveAsync(moveBy: currentPlayer.Color, gameToken, game);
        _overtime.TryEndOvertimeTurn(currentPlayer.Color, game.OvertimeState);
        StartNextOvertimeTurn(sideToMove: nextPlayer.Color, game);
        return moveResult.EndStatus;
    }

    private MoveSnapshot BuildAndStoreMove(
        GameColor movedBy,
        GameColor nextPlayer,
        MoveResult moveResult,
        GameData game
    )
    {
        var timeLeft = _clock.CommitTurn(movedBy, game.ClockState);
        MoveSnapshot snapshot = game.MoveHistory.AddMove(nextPlayer, moveResult, timeLeft);
        return snapshot;
    }

    private async Task HandleDrawForMoveAsync(GameColor moveBy, GameToken gameToken, GameData game)
    {
        game.DrawRequest.DecrementCooldown();
        // auto decline the draw if it exists
        if (game.DrawRequest.TryDeclineDraw(moveBy, _settings.DrawCooldown))
        {
            await _notifier.NotifyDrawStateChangeAsync(
                gameToken,
                game.DrawRequest.GetState(),
                game.NotifierState
            );
        }
    }

    private void StartNextOvertimeTurn(GameColor sideToMove, GameData game)
    {
        if (!_clock.IsTimeout(sideToMove, isTicking: true, game.ClockState))
        {
            return;
        }
        _overtime.StartOvertimeTurn(sideToMove, game.OvertimeState);
    }
}
