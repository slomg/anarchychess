using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
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
    IGameNotifier gameNotifier,
    IGameFinalizer gameFinalizer
) : IGameEndHandler
{
    private readonly IGameCore _core = core;
    private readonly IGameClock _clock = clock;
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

        var result = await _gameFinalizer.FinalizeGameAsync(gameToken, state, endStatus, token);
        await _gameNotifier.NotifyGameEndedAsync(
            gameToken,
            result,
            _clock.ToSnapshot(game.ClockState),
            game.NotifierState
        );

        return result;
    }
}
