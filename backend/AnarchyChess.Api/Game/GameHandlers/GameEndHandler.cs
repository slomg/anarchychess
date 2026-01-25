using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.GameHandlers;

public interface IGameEndHandler
{
    Task<GameResultData> HandleGameEndAsync(
        GameState state,
        GameEndStatus endStatus,
        GameToken gameToken,
        GameData game,
        CancellationToken token = default
    );
}

public class GameEndHandler(
    IGameCore core,
    IGameClock clock,
    IOvertime overtime,
    IGameNotifier gameNotifier,
    IGameFinalizer gameFinalizer
) : IGameEndHandler
{
    private readonly IGameCore _core = core;
    private readonly IGameClock _clock = clock;
    private readonly IOvertime _overtime = overtime;
    private readonly IGameNotifier _gameNotifier = gameNotifier;
    private readonly IGameFinalizer _gameFinalizer = gameFinalizer;

    public async Task<GameResultData> HandleGameEndAsync(
        GameState state,
        GameEndStatus endStatus,
        GameToken gameToken,
        GameData game,
        CancellationToken token = default
    )
    {
        var sideToMove = _core.SideToMove(game.Core);
        _clock.CommitLastTurn(sideToMove, game.ClockState);
        RemoveOvertimePieces(sideToMove, game);

        var result = await _gameFinalizer.FinalizeGameAsync(gameToken, state, endStatus, token);
        await _gameNotifier.NotifyGameEndedAsync(
            gameToken,
            result,
            _clock.ToSnapshot(game.ClockState),
            game.NotifierState
        );

        return result;
    }

    private void RemoveOvertimePieces(GameColor sideToMove, GameData game)
    {
        var (pendingRemoval, newLegalMoves) = _overtime.ConsumeOvertimeRemovals(
            sideToMove,
            game.OvertimeState
        );
        if (pendingRemoval.Count == 0)
        {
            return;
        }

        _core.RemovePieces(pendingRemoval, newLegalMoves, game.Core);
        var board = _core.GetReadOnlyBoard(game.Core);
        game.MoveHistory.CommitOvertimeRemovals(pendingRemoval, board.Width);

        _overtime.EndOvertime(game.OvertimeState);
    }
}
