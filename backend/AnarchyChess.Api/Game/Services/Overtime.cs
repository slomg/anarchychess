using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Services;

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
    ) ProcessOvertimeRemovals(GameColor playerColor, OvertimeState state);
    bool HasStartedOvertime(GameColor playerColor, OvertimeState state);
    TimeSpan GetTimeUntilDefeat(GameColor playerColor, OvertimeState state);
    long GetOvertimeTurnStartedAt(OvertimeState state);
    double GetPlayerSecondRemainderMs(GameColor playerColor, OvertimeState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.PendingRemovalEntry")]
public readonly record struct PendingRemovalEntry(AlgebraicPoint Position, LegalMoveSet LegalMoves);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.OvertimeState")]
public class OvertimeState
{
    [Id(0)]
    public Dictionary<GameColor, PlayerOvertime> PlayerOvertime { get; } = [];

    [Id(1)]
    public long OvertimeTurnStartedAt { get; set; }
}

public class Overtime(
    IRandomProvider randomProvider,
    TimeProvider timeProvider,
    IPlayableMoveProvider playableMoveProvider,
    IMoveEncoder moveEncoder
) : IOvertime
{
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IPlayableMoveProvider _playableMoveProvider = playableMoveProvider;
    private readonly IMoveEncoder _moveEncoder = moveEncoder;

    public OvertimeSnapshot ToSnapshot(OvertimeState state)
    {
        var whiteOvertime = BuildPlayerOvertimeSnapshot(GameColor.White, state);
        var blackOvertime = BuildPlayerOvertimeSnapshot(GameColor.Black, state);

        return new(
            WhiteOvertime: whiteOvertime,
            BlackOvertime: blackOvertime,
            OvertimeTurnStartedAt: state.OvertimeTurnStartedAt
        );
    }

    private static PlayerOvertimePathSnapshot? BuildPlayerOvertimeSnapshot(
        GameColor playerColor,
        OvertimeState state
    )
    {
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        if (playerOvertime is null)
        {
            return null;
        }

        List<PendingOvertimeRemovalPathSnapshot> pendingRemoval =
        [
            .. playerOvertime.PendingRemoval.Select(x => new PendingOvertimeRemovalPathSnapshot(
                LegalMoves: x.LegalMoves.MovePaths,
                RemovedPiece: x.Position
            )),
        ];
        return new(
            SecondRemainderMs: playerOvertime.SecondRemainderMs,
            PendingRemoval: pendingRemoval
        );
    }

    public List<OvertimePendingRemovalNotification> StartOvertimeTurn(
        GameColor overtimedPlayerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    )
    {
        state.OvertimeTurnStartedAt = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        ChessBoard editingBoard = new(board);
        List<OvertimePendingRemovalNotification> result = [];
        List<PendingRemovalEntry> pendingRemoval = [];
        foreach (var position in ComputeOvertimeRemovals(overtimedPlayerColor, board))
        {
            editingBoard.RemovePiece(position);

            var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(editingBoard);
            var encoded = _moveEncoder.EncodeMoves(legalMoves.MovePaths);

            result.Add(
                new OvertimePendingRemovalNotification(
                    EncodedLegalMoves: encoded,
                    RemovePieceAt: position
                )
            );
            pendingRemoval.Add(new(position, legalMoves));
        }

        if (state.PlayerOvertime.TryGetValue(overtimedPlayerColor, out var playerOvertime))
        {
            playerOvertime.PendingRemoval = pendingRemoval;
        }
        else
        {
            state.PlayerOvertime[overtimedPlayerColor] = new() { PendingRemoval = pendingRemoval };
        }

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

        double timeSinceLastMove =
            _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.OvertimeTurnStartedAt;
        timeSinceLastMove += playerOvertime.SecondRemainderMs;
        playerOvertime.SecondRemainderMs = timeSinceLastMove % 1000;

        List<AlgebraicPoint> pendingRemoval = [];
        double secondsSinceLastMove = timeSinceLastMove / 1000;
        int removedCount = Math.Min(playerOvertime.PendingRemoval.Count, (int)secondsSinceLastMove);
        for (int i = 0; i < removedCount; i++)
        {
            pendingRemoval.Add(playerOvertime.PendingRemoval[i].Position);
        }

        if (removedCount >= playerOvertime.PendingRemoval.Count)
        {
            return (
                PendingRemoval: pendingRemoval,
                NewLegalMoves: new LegalMoveSet(),
                IsGameOver: true
            );
        }
        else
        {
            return (
                PendingRemoval: pendingRemoval,
                NewLegalMoves: playerOvertime.PendingRemoval[removedCount - 1].LegalMoves,
                IsGameOver: false
            );
        }
    }

    public (
        List<AlgebraicPoint> PendingRemoval,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) ProcessOvertimeRemovals(GameColor playerColor, OvertimeState state)
    {
        var result = GetRemovedPiecesSinceLastMove(playerColor, state);
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        playerOvertime?.PendingRemoval =
        [
            .. playerOvertime.PendingRemoval.Skip(result.PendingRemoval.Count),
        ];

        return result;
    }

    public long GetOvertimeTurnStartedAt(OvertimeState state) => state.OvertimeTurnStartedAt;

    public double GetPlayerSecondRemainderMs(GameColor playerColor, OvertimeState state) =>
        state.PlayerOvertime.GetValueOrDefault(playerColor)?.SecondRemainderMs ?? 0;

    public bool HasStartedOvertime(GameColor playerColor, OvertimeState state) =>
        state.PlayerOvertime.ContainsKey(playerColor);

    public TimeSpan GetTimeUntilDefeat(GameColor playerColor, OvertimeState state)
    {
        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor);
        if (playerOvertime is null || playerOvertime.PendingRemoval.Count == 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(playerOvertime.PendingRemoval.Count)
            - TimeSpan.FromMilliseconds(playerOvertime.SecondRemainderMs);
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
