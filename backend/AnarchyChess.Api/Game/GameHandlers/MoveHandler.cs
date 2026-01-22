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
    IGameNotifier notifier
) : IMoveHandler
{
    private readonly ILogger<MoveHandler> _logger = logger;
    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IGameCore _core = core;
    private readonly IGameClock _clock = clock;
    private readonly IGameNotifier _notifier = notifier;

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
                PlyNumber: game.MoveSnapshots.Count,
                Clocks: _clock.ToSnapshot(game.ClockState),
                SideToMoveUserId: nextPlayer.UserId,
                EncodedLegalMoves: _core.EncodeLegalMoves(game.Core),
                DidMoveEndGame: moveResult.EndStatus is not null
            ),
            game.NotifierState
        );

        await HandleDrawForMoveAsync(moveBy: currentPlayer.Color, gameToken, game);
        var timeoutStatus = _clock.DetectTimeout(
            tickingPlayer: _core.SideToMove(game.Core),
            game.ClockState
        );

        return moveResult.EndStatus ?? timeoutStatus;
    }

    private MoveSnapshot BuildAndStoreMove(
        GameColor movedBy,
        GameColor nextPlayer,
        MoveResult moveResult,
        GameData game
    )
    {
        var timeLeft = _clock.CommitTurn(movedBy, game.ClockState);

        MoveSnapshot moveSnapshot = new(
            Path: moveResult.MovePath,
            Fen: moveResult.Fen.FullFen,
            NextSideToMove: nextPlayer,
            San: moveResult.San,
            timeLeft
        );
        game.MoveSnapshots.Add(moveSnapshot);
        return moveSnapshot;
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
}
