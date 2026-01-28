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
            return _overtime.GetTimeUntilNextRemoval(sideToMove, game.OvertimeState);
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

        var player = game.Players.GetPlayerByColor(playerColor);
        if (!_overtime.HasEnteredOvertime(playerColor, game.OvertimeState))
        {
            await StartOvertimeAsync(player, gameToken, game);
            return null;
        }

        var result = await TryRemoveNextAsync(player, sideToMove: sideToMove, gameToken, game);
        return result;
    }

    private async Task StartOvertimeAsync(GamePlayer player, GameToken gameToken, GameData game)
    {
        var board = _core.GetReadOnlyBoard(game.Core);
        var nextRemoval = _overtime.StartOvertimeTurn(player.Color, board, game.OvertimeState);
        if (nextRemoval is not null)
        {
            await _notifier.NotifyNextOvertimeAsync(
                player.UserId,
                plyNumber: game.MoveHistory.Moves.Count,
                removeFrom: nextRemoval.Value,
                gameToken: gameToken
            );
        }
    }

    private async Task<GameEndStatus?> TryRemoveNextAsync(
        GamePlayer player,
        GameColor sideToMove,
        GameToken gameToken,
        GameData game
    )
    {
        if (player.Color != sideToMove)
        {
            return null;
        }

        var board = _core.GetReadOnlyBoard(game.Core);
        var (removalResult, isGameOver) = _overtime.GetNextRemoval(
            player.Color,
            board,
            game.OvertimeState
        );
        var endStatus = isGameOver ? _resultDescriber.Overtime(player.Color) : null;
        if (removalResult is null)
        {
            return endStatus;
        }

        int plyNumber = game.MoveHistory.Moves.Count;

        await _notifier.NotifyOvertimeAsync(
            plyNumber,
            removalResult.RemoveFrom,
            removalResult.EncodedLegalMoves,
            gameToken,
            game.NotifierState
        );
        game.MoveHistory.CommitOvertimeRemoval(removalResult.RemoveFrom, boardWidth: board.Width);
        _core.RemovePiece(removalResult.RemoveFrom, removalResult.NewLegalMoves, game.Core);

        if (removalResult.NextRemoval is not null)
        {
            await _notifier.NotifyNextOvertimeAsync(
                player.UserId,
                plyNumber,
                removeFrom: removalResult.NextRemoval.Value,
                gameToken: gameToken
            );
        }
        return endStatus;
    }
}
