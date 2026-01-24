using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Services;

namespace AnarchyChess.Api.Game.Services;

public interface IOvertime
{
    OvertimeSnapshot ToSnapshot(OvertimeState state);
    List<OvertimePosition> StartOvertimeTurn(
        GameColor overtimedPlayerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    );
    (
        List<AlgebraicPoint> Positions,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) GetRemovedPiecesSinceLastMove(GameColor playerColor, OvertimeState state);
    bool HasStartedOvertime(GameColor playerColor, OvertimeState state);
    TimeSpan GetTimeUntilDefeat(GameColor playerColor, OvertimeState state);
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.PendingRemovalEntry")]
public readonly record struct PendingRemovalEntry(AlgebraicPoint Position, LegalMoveSet LegalMoves);

public record OvertimePosition(IReadOnlyList<byte> EncodedLegalMoves, AlgebraicPoint RemovedPiece);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.OvertimeState")]
public class OvertimeState
{
    [Id(0)]
    public Dictionary<GameColor, IReadOnlyList<PendingRemovalEntry>> PendingRemoval { get; } = [];

    [Id(1)]
    public long LastMoveAtMs { get; set; }

    [Id(2)]
    public Dictionary<GameColor, double> SecondRemainder { get; } =
        new() { [GameColor.White] = 0, [GameColor.Black] = 0 };
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
        var whiteOvertime = state
            .PendingRemoval.GetValueOrDefault(GameColor.White, [])
            .Select(x => new EncodedOvertimePositionSnapshot(
                LegalMoves: x.LegalMoves.MovePaths,
                RemovedPiece: x.Position
            ))
            .ToList();

        var blackOvertime = state
            .PendingRemoval.GetValueOrDefault(GameColor.Black, [])
            .Select(x => new EncodedOvertimePositionSnapshot(
                LegalMoves: x.LegalMoves.MovePaths,
                RemovedPiece: x.Position
            ))
            .ToList();

        return new(WhiteOvertime: whiteOvertime, BlackOvertime: blackOvertime);
    }

    public List<OvertimePosition> StartOvertimeTurn(
        GameColor overtimedPlayerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    )
    {
        state.LastMoveAtMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        ChessBoard editingBoard = new(board);
        List<OvertimePosition> result = [];
        List<PendingRemovalEntry> pendingRemoval = [];
        foreach (var position in ComputeOvertimeRemovals(overtimedPlayerColor, board))
        {
            editingBoard.RemovePiece(position);

            var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(editingBoard);
            var encoded = _moveEncoder.EncodeMoves(legalMoves.MovePaths);

            result.Add(new OvertimePosition(EncodedLegalMoves: encoded, RemovedPiece: position));
            pendingRemoval.Add(new(position, legalMoves));
        }
        state.PendingRemoval[overtimedPlayerColor] = pendingRemoval;

        return result;
    }

    public (
        List<AlgebraicPoint> Positions,
        LegalMoveSet NewLegalMoves,
        bool IsGameOver
    ) GetRemovedPiecesSinceLastMove(GameColor playerColor, OvertimeState state)
    {
        var pendingRemoval = state.PendingRemoval.GetValueOrDefault(playerColor);
        if (pendingRemoval is null)
        {
            return (Positions: [], NewLegalMoves: new LegalMoveSet(), IsGameOver: false);
        }

        double timeSinceLastMove =
            _timeProvider.GetUtcNow().ToUnixTimeMilliseconds() - state.LastMoveAtMs;
        timeSinceLastMove += state.SecondRemainder[playerColor];

        double secondsSinceLastMove = timeSinceLastMove / 1000;
        state.SecondRemainder[playerColor] = secondsSinceLastMove % 1;

        int removedCount = (int)secondsSinceLastMove;

        List<AlgebraicPoint> positions = [];
        for (int i = 0; i < Math.Min(removedCount, pendingRemoval.Count); i++)
        {
            positions.Add(pendingRemoval[i].Position);
        }

        if (removedCount >= pendingRemoval.Count)
        {
            return (Positions: positions, NewLegalMoves: new LegalMoveSet(), IsGameOver: true);
        }
        else
        {
            return (
                Positions: positions,
                NewLegalMoves: pendingRemoval[removedCount].LegalMoves,
                IsGameOver: false
            );
        }
    }

    public bool HasStartedOvertime(GameColor playerColor, OvertimeState state) =>
        state.PendingRemoval.ContainsKey(playerColor);

    public TimeSpan GetTimeUntilDefeat(GameColor playerColor, OvertimeState state)
    {
        var pendingRemovals = state.PendingRemoval.GetValueOrDefault(playerColor);
        if (pendingRemovals is null || pendingRemovals.Count == 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(pendingRemovals.Count - state.SecondRemainder[playerColor]);
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
            var (position, _) = nonKings[firstIdx];
            yield return position;

            squares[firstIdx] = squares[^1];
            squares.RemoveAt(squares.Count - 1);
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
