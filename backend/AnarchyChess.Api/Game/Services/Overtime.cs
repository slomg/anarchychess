using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using Microsoft.Extensions.Options;
using System.Data;

namespace AnarchyChess.Api.Game.Services;

public interface IOvertime
{
    void StartOvertimeTurn(GameColor playerColor, OvertimeState state);
    void TryEndOvertimeTurn(GameColor playerColor, OvertimeState state);
    TimeSpan GetTimeUntilNextRemoval(GameColor playerColor, OvertimeState state);
    (OvertimeRemovalResult? RemovalResult, bool IsGameOver) GetNextRemoval(
        GameColor playerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    );
    bool HasEnteredOvertime(GameColor playerColor, OvertimeState state);
}

public record OvertimeRemovalResult(
    AlgebraicPoint RemoveFrom,
    LegalMoveSet NewLegalMoves,
    CompressedMoves EncodedLegalMoves
);

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

    public (OvertimeRemovalResult? RemovalResult, bool IsGameOver) GetNextRemoval(
        GameColor playerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    )
    {
        if (!state.PlayersEnteredOvertime.Contains(playerColor))
        {
            return (RemovalResult: null, IsGameOver: false);
        }
        state.PlayerRemainder[playerColor] = TimeSpan.Zero;

        List<(AlgebraicPoint Position, Piece Occupant)> pieces =
        [
            .. board.EnumeratePieces().Where(x => x.Occupant.Color == playerColor),
        ];
        if (pieces.Count == 0)
        {
            return (RemovalResult: null, IsGameOver: true);
        }
        var removeFrom = _randomProvider
            .NextItemWeighted(
                pieces,
                piece =>
                    piece.Occupant.Type switch
                    {
                        PieceType type when GameLogicConstants.PawnLikePieces.Contains(type) => 4,
                        PieceType.Queen => 2,
                        PieceType.King => 1,
                        _ => 3,
                    }
            )
            .Position;

        ChessBoard editingBoard = new(board);
        editingBoard.RemovePiece(removeFrom);

        int kingCount = editingBoard.GetAllPiecesWith(PieceType.King, playerColor).Count;
        var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(editingBoard);
        var encoded = _moveEncoder.EncodeMoves(legalMoves.MovePaths);

        OvertimeRemovalResult result = new(
            RemoveFrom: removeFrom,
            NewLegalMoves: legalMoves,
            EncodedLegalMoves: encoded
        );
        return (RemovalResult: result, IsGameOver: kingCount == 0);
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
