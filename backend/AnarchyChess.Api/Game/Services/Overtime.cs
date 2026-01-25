using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IOvertime
{
    OvertimeSnapshot ToSnapshot(OvertimeState state);
    List<OvertimePendingRemovalNotification> StartOvertimeTurn(
        GameColor overtimedPlayerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    );
    (
        List<AlgebraicPoint> PendingRemoval,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) GetRemovedPiecesSinceLastMove(GameColor playerColor, OvertimeState state);
    (
        List<AlgebraicPoint> PendingRemoval,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) ConsumeOvertimeRemovals(GameColor playerColor, OvertimeState state);
    bool HasStartedOvertime(GameColor playerColor, OvertimeState state);
    TimeSpan GetTimeUntilDefeat(GameColor playerColor, OvertimeState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.PendingRemovalEntry")]
public readonly record struct PendingRemovalEntry(
    AlgebraicPoint RemoveFrom,
    LegalMoveSet LegalMoves,
    long RemoveAtTimestamp
);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.OvertimeState")]
public class OvertimeState
{
    [Id(0)]
    public Dictionary<GameColor, PlayerOvertime> PlayerOvertime { get; } = [];
}

public class Overtime(
    IOptions<AppSettings> settings,
    IRandomProvider randomProvider,
    TimeProvider timeProvider,
    IPlayableMoveProvider playableMoveProvider,
    IMoveEncoder moveEncoder
) : IOvertime
{
    private readonly GameSettings _settings = settings.Value.Game;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IPlayableMoveProvider _playableMoveProvider = playableMoveProvider;
    private readonly IMoveEncoder _moveEncoder = moveEncoder;

    public OvertimeSnapshot ToSnapshot(OvertimeState state)
    {
        var whiteOvertime = BuildPlayerOvertimeSnapshot(GameColor.White, state);
        var blackOvertime = BuildPlayerOvertimeSnapshot(GameColor.Black, state);

        return new(WhiteOvertime: whiteOvertime, BlackOvertime: blackOvertime);
    }

    private static List<PendingOvertimeRemovalPathSnapshot>? BuildPlayerOvertimeSnapshot(
        GameColor playerColor,
        OvertimeState state
    )
    {
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        if (playerOvertime is null)
        {
            return null;
        }

        return
        [
            .. playerOvertime.PendingRemoval.Select(x => new PendingOvertimeRemovalPathSnapshot(
                LegalMoves: x.LegalMoves.MovePaths,
                RemoveFrom: x.RemoveFrom,
                RemoveAtTimestamp: x.RemoveAtTimestamp
            )),
        ];
    }

    public List<OvertimePendingRemovalNotification> StartOvertimeTurn(
        GameColor overtimedPlayerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    )
    {
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(
            overtimedPlayerColor,
            new() { PendingRemoval = [] }
        );

        var prevRemoval =
            _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - playerOvertime.RemainderMs;
        ChessBoard editingBoard = new(board);
        List<OvertimePendingRemovalNotification> result = [];
        List<PendingRemovalEntry> pendingRemoval = [];
        foreach (var position in ComputeOvertimeRemovals(overtimedPlayerColor, board))
        {
            editingBoard.RemovePiece(position);

            var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(editingBoard);
            var encoded = _moveEncoder.EncodeMoves(legalMoves.MovePaths);

            long removeAtTimestamp =
                prevRemoval + (long)_settings.OvertimeRemovalInterval.TotalMilliseconds;
            result.Add(
                new OvertimePendingRemovalNotification(
                    EncodedLegalMoves: encoded,
                    RemoveFrom: position,
                    RemoveAtTimestamp: removeAtTimestamp
                )
            );
            pendingRemoval.Add(new(position, legalMoves, RemoveAtTimestamp: removeAtTimestamp));
            prevRemoval = removeAtTimestamp;
        }

        playerOvertime.PendingRemoval = pendingRemoval;
        state.PlayerOvertime[overtimedPlayerColor] = playerOvertime;

        return result;
    }

    public (
        List<AlgebraicPoint> PendingRemoval,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) GetRemovedPiecesSinceLastMove(GameColor playerColor, OvertimeState state)
    {
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        if (playerOvertime is null)
        {
            return (PendingRemoval: [], NewLegalMoves: new LegalMoveSet(), IsGameOver: false);
        }

        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        List<PendingRemovalEntry> removed =
        [
            .. playerOvertime.PendingRemoval.TakeWhile(x => x.RemoveAtTimestamp <= nowMs),
        ];

        var newLegalMoves = removed.Count > 0 ? removed[^1].LegalMoves : new LegalMoveSet();
        bool isGameOver = removed.Count >= playerOvertime.PendingRemoval.Count;
        return (
            PendingRemoval: [.. removed.Select(x => x.RemoveFrom)],
            NewLegalMoves: newLegalMoves,
            IsGameOver: isGameOver
        );
    }

    public (
        List<AlgebraicPoint> PendingRemoval,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) ConsumeOvertimeRemovals(GameColor playerColor, OvertimeState state)
    {
        var result = GetRemovedPiecesSinceLastMove(playerColor, state);
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        if (playerOvertime is null)
        {
            return result;
        }

        playerOvertime.PendingRemoval =
        [
            .. playerOvertime.PendingRemoval.Skip(result.PendingRemoval.Count),
        ];
        if (playerOvertime.PendingRemoval.Count == 0)
        {
            playerOvertime.RemainderMs = 0;
            return result;
        }

        var next = playerOvertime.PendingRemoval[0];
        playerOvertime.RemainderMs = Math.Max(
            0,
            next.RemoveAtTimestamp - _timeProvider.GetUtcNow().ToUnixTimeMilliseconds()
        );

        return result;
    }

    public bool HasStartedOvertime(GameColor playerColor, OvertimeState state) =>
        state.PlayerOvertime.ContainsKey(playerColor);

    public TimeSpan GetTimeUntilDefeat(GameColor playerColor, OvertimeState state)
    {
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        if (playerOvertime is null || playerOvertime.PendingRemoval.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        var defeatAtMs = playerOvertime.PendingRemoval[^1].RemoveAtTimestamp;

        var remainingMs = Math.Max(0, defeatAtMs - nowMs);
        return TimeSpan.FromMilliseconds(remainingMs);
    }

    private IEnumerable<AlgebraicPoint> ComputeOvertimeRemovals(
        GameColor overtimedPlayerColor,
        IReadOnlyChessBoard board
    )
    {
        List<(AlgebraicPoint position, Piece occupant)> squares =
        [
            .. board.EnumeratePieces().Where(x => x.Occupant.Color == overtimedPlayerColor),
        ];

        // make sure the king is not picked first
        var nonKings = squares.Where(x => x.occupant.Type is not PieceType.King).ToList();
        if (nonKings.Count > 0)
        {
            var firstIdx = _randomProvider.Next(nonKings.Count);
            var picked = nonKings[firstIdx];
            yield return picked.position;

            squares.Remove(picked);
        }

        int kingCount = board.GetAllPiecesWith(PieceType.King, overtimedPlayerColor).Count;
        while (kingCount > 0 && squares.Count > 0)
        {
            var squareIdx = _randomProvider.Next(squares.Count);
            var (position, occupant) = squares[squareIdx];
            yield return position;

            squares[squareIdx] = squares[^1];
            squares.RemoveAt(squares.Count - 1);

            if (occupant.Type is PieceType.King)
            {
                kingCount--;
            }
        }
    }
}
