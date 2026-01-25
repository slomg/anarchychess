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
                "User {UserId} attmpted to move a piece, but their id doesn't match the current player {PlayingUserId}",
                moveMadeBy,
                currentPlayer?.UserId
            );
            return GameErrors.PlayerInvalid;
        }

        var overtimeRemovals = RemovePiecesForOvertime(currentPlayer: currentPlayer.Color, game);
        var makeMoveResult = _core.MakeMove(key, game.Core);
        if (makeMoveResult.IsError)
            return makeMoveResult.Errors;
        var moveResult = makeMoveResult.Value;

        var nextPlayer = game.Players.GetPlayerByColor(_core.SideToMove(game.Core));
        var moveSnapshot = BuildAndStoreMove(
            movedBy: currentPlayer.Color,
            nextPlayer: nextPlayer.Color,
            moveResult,
            overtimeRemovals,
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
        await StartNextOvertimeTurnAsync(sideToMove: nextPlayer.Color, gameToken, game);
        return moveResult.EndStatus;
    }

    private MoveSnapshot BuildAndStoreMove(
        GameColor movedBy,
        GameColor nextPlayer,
        MoveResult moveResult,
        List<AlgebraicPoint> overtimeRemovals,
        GameData game
    )
    {
        var timeLeft = _clock.CommitTurn(movedBy, game.ClockState);

        MoveSnapshot snapshot;
        if (overtimeRemovals.Count > 0)
        {
            var board = _core.GetReadOnlyBoard(game.Core);
            snapshot = game.MoveHistory.AddMoveWithOvertimeRemovals(
                nextPlayer,
                moveResult,
                timeLeft,
                overtimeRemovals,
                board.Width
            );
        }
        else
        {
            snapshot = game.MoveHistory.AddMove(nextPlayer, moveResult, timeLeft);
        }

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

    private List<AlgebraicPoint> RemovePiecesForOvertime(GameColor currentPlayer, GameData game)
    {
        if (!_overtime.HasStartedOvertime(currentPlayer, game.OvertimeState))
        {
            return [];
        }

        var (pendingRemovals, newLegalMoves) = _overtime.ConsumeOvertimeRemovals(
            currentPlayer,
            game.OvertimeState
        );
        _core.RemovePieces(pendingRemovals, newLegalMoves, game.Core);
        return pendingRemovals;
    }

    private async Task StartNextOvertimeTurnAsync(
        GameColor sideToMove,
        GameToken gameToken,
        GameData game
    )
    {
        if (!_clock.IsTimeout(sideToMove, isTicking: true, game.ClockState))
        {
            return;
        }

        var pendingRemoval = _overtime.StartOvertimeTurn(
            sideToMove,
            _core.GetReadOnlyBoard(game.Core),
            game.OvertimeState
        );
        await _notifier.NotifyOvertimeAsync(
            sideToMove,
            pendingRemoval,
            gameToken,
            game.NotifierState
        );
    }
}
