using System.Data;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IOvertime
{
    void StartOvertimeTurn(GameColor playerColor, OvertimeState state);
    void TryEndOvertimeTurn(GameColor playerColor, OvertimeState state);
    TimeSpan GetTimeUntilNextRemoval(GameColor playerColor, OvertimeState state);
    (OvertimeRemovalNotification? Notification, bool IsGameOver) GetNextRemoval(
        GameColor playerColor,
        IReadOnlyChessBoard board
    );
    bool HasEnteredOvertime(GameColor playerColor, OvertimeState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.OvertimeState")]
public class OvertimeState
{
    [Id(0)]
    public Dictionary<GameColor, TimeSpan> PlayerRemainder { get; } = [];

    [Id(1)]
    public HashSet<GameColor> PlayersEnteredOvertime { get; } = [];

    [Id(2)]
    public long LastMoveAtTimestamp { get; set; }
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

    public (OvertimeRemovalNotification? Notification, bool IsGameOver) GetNextRemoval(
        GameColor playerColor,
        IReadOnlyChessBoard board
    )
    {
        List<AlgebraicPoint> squares =
        [
            .. board
                .EnumeratePieces()
                .Where(x => x.Occupant.Color == playerColor)
                .Select(x => x.Position),
        ];
        if (squares.Count == 0)
        {
            return (Notification: null, IsGameOver: true);
        }

        var squareIdx = _randomProvider.Next(squares.Count);
        var position = squares[squareIdx];
        ChessBoard editingBoard = new(board);
        editingBoard.RemovePiece(position);

        int kingCount = editingBoard.GetAllPiecesWith(PieceType.King, playerColor).Count;
        var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(editingBoard);
        var encoded = _moveEncoder.EncodeMoves(legalMoves.MovePaths);
        return (
            Notification: new OvertimeRemovalNotification(
                EncodedLegalMoves: encoded,
                RemoveFrom: position
            ),
            IsGameOver: kingCount == 0
        );
    }

    public void StartOvertimeTurn(GameColor playerColor, OvertimeState state)
    {
        state.LastMoveAtTimestamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        state.PlayersEnteredOvertime.Add(playerColor);
    }

    public void TryEndOvertimeTurn(GameColor playerColor, OvertimeState state)
    {
        if (!state.PlayersEnteredOvertime.Contains(playerColor))
        {
            return;
        }

        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        var distanceFromLastMove = nowMs - state.LastMoveAtTimestamp;
        state.PlayerRemainder[playerColor] = TimeSpan.FromMilliseconds(
            distanceFromLastMove % _settings.OvertimeRemovalInterval.TotalMilliseconds
        );
    }

    public TimeSpan GetTimeUntilNextRemoval(GameColor playerColor, OvertimeState state)
    {
        TimeSpan remainder = state.PlayerRemainder.GetValueOrDefault(playerColor);
        return _settings.OvertimeRemovalInterval - remainder;
    }

    public bool HasEnteredOvertime(GameColor playerColor, OvertimeState state) =>
        state.PlayersEnteredOvertime.Contains(playerColor);
}
