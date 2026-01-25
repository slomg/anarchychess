using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.GameHandlers;

public interface IClockHandler
{
    TimeSpan GetClockDueTime(GameData game);
    Task<(TimeSpan? RescheduleTo, GameEndStatus? EndResult)> OnClockTickAsync(
        GameToken gameToken,
        GameData game
    );
}

public class ClockHandler(
    IGameClock clock,
    IGameCore core,
    IOvertime overtime,
    IGameNotifier notifier,
    IGameResultDescriber resultDescriber
) : IClockHandler
{
    private readonly IGameClock _clock = clock;
    private readonly IGameCore _core = core;
    private readonly IOvertime _overtime = overtime;
    private readonly IGameNotifier _notifier = notifier;
    private readonly IGameResultDescriber _resultDescriber = resultDescriber;

    public async Task<(TimeSpan? RescheduleTo, GameEndStatus? EndResult)> OnClockTickAsync(
        GameToken gameToken,
        GameData game
    )
    {
        var whiteReslut = await HandlePlayerOvertimeAsync(GameColor.White, gameToken, game);
        if (whiteReslut != null)
        {
            return (RescheduleTo: null, EndResult: whiteReslut);
        }

        var blackReslut = await HandlePlayerOvertimeAsync(GameColor.Black, gameToken, game);
        if (blackReslut != null)
        {
            return (RescheduleTo: null, EndResult: blackReslut);
        }

        var rescheduleTo = GetClockDueTime(game);
        return (RescheduleTo: rescheduleTo, EndResult: null);
    }

    public TimeSpan GetClockDueTime(GameData game)
    {
        var sideToMove = _core.SideToMove(game.Core);
        if (_clock.IsTimeout(sideToMove, isTicking: true, game.ClockState))
        {
            return _overtime.GetTimeUntilDefeat(sideToMove, game.OvertimeState);
        }
        else
        {
            var timeLeftMs = _clock.CalculateTimeLeftMs(sideToMove, game.ClockState);
            return TimeSpan.FromMilliseconds(timeLeftMs);
        }
    }

    private async Task<GameEndStatus?> HandlePlayerOvertimeAsync(
        GameColor playerColor,
        GameToken gameToken,
        GameData game
    )
    {
        var sideToMove = _core.SideToMove(game.Core);
        bool hasTimedOut = _clock.IsTimeout(
            playerColor,
            isTicking: playerColor == sideToMove,
            game.ClockState
        );
        if (!hasTimedOut)
        {
            return null;
        }

        if (_clock.IsInGracePeriod(playerColor, game.ClockState))
        {
            return _resultDescriber.Aborted(by: playerColor);
        }

        if (!_overtime.HasStartedOvertime(playerColor, game.OvertimeState))
        {
            await StartOvertimeAsync(playerColor, gameToken, game);
            return null;
        }

        var (pendingRemoval, newLegalMoves, isGameOver) = _overtime.ConsumeOvertimeRemovals(
            playerColor,
            game.OvertimeState
        );
        _core.RemovePieces(pendingRemoval, newLegalMoves, game.Core);

        return isGameOver ? _resultDescriber.Overtime(playerColor) : null;
    }

    private async Task StartOvertimeAsync(
        GameColor overtimedPlayer,
        GameToken gameToken,
        GameData game
    )
    {
        var pendingRemoval = _overtime.StartOvertimeTurn(
            overtimedPlayer,
            _core.GetReadOnlyBoard(game.Core),
            game.OvertimeState
        );
        await _notifier.NotifyOvertimeAsync(
            overtimedPlayer,
            pendingRemoval,
            gameToken,
            game.NotifierState
        );
    }
}
