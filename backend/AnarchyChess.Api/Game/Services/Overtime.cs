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
    AlgebraicPoint? StartOvertimeTurn(
        GameColor playerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    );
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
    AlgebraicPoint? NextRemoval,
    LegalMoveSet NewLegalMoves,
    CompressedMoves EncodedLegalMoves
);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.OvertimeState")]
public class OvertimeState
{
    [Id(0)]
    public Dictionary<GameColor, PlayerOvertime> PlayerOvertime { get; } = [];

    [Id(1)]
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
        if (!state.PlayerOvertime.TryGetValue(playerColor, out var playerOvertime))
        {
            return (RemovalResult: null, IsGameOver: false);
        }
        playerOvertime.Remainder = TimeSpan.Zero;

        var removeFrom = PickNextRemoval(playerColor, board, playerOvertime);
        if (removeFrom is null)
        {
            return (RemovalResult: null, IsGameOver: true);
        }

        ChessBoard editingBoard = new(board);
        editingBoard.RemovePiece(removeFrom.RemoveFrom);

        int kingCount = editingBoard.GetAllPiecesWith(PieceType.King, playerColor).Count;
        var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(editingBoard);
        var encoded = _moveEncoder.EncodeMoves(legalMoves.MovePaths);
        bool isGameOver = kingCount == 0;

        var nextRemoval = isGameOver ? null : PickRandomPiece(playerColor, editingBoard);
        playerOvertime.PickedNextRemoval = nextRemoval;

        OvertimeRemovalResult result = new(
            RemoveFrom: removeFrom.RemoveFrom,
            NextRemoval: nextRemoval?.RemoveFrom,
            NewLegalMoves: legalMoves,
            EncodedLegalMoves: encoded
        );
        return (RemovalResult: result, IsGameOver: isGameOver);
    }

    public AlgebraicPoint? StartOvertimeTurn(
        GameColor playerColor,
        IReadOnlyChessBoard board,
        OvertimeState state
    )
    {
        state.LastMoveAtTimestamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        var playerOvertime = state.PlayerOvertime.GetValueOrDefault(playerColor, new());

        playerOvertime.PickedNextRemoval = PickNextRemoval(playerColor, board, playerOvertime);
        state.PlayerOvertime[playerColor] = playerOvertime;
        return playerOvertime.PickedNextRemoval?.RemoveFrom;
    }

    public void TryEndOvertimeTurn(GameColor playerColor, OvertimeState state)
    {
        if (!state.PlayerOvertime.TryGetValue(playerColor, out var playerOvertime))
        {
            return;
        }

        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        var distanceFromLastMove = nowMs - state.LastMoveAtTimestamp;
        playerOvertime.Remainder += TimeSpan.FromMilliseconds(
            distanceFromLastMove % _settings.InitialOvertimeRemovalInterval.TotalMilliseconds
        );
        if (playerOvertime.Remainder > _settings.InitialOvertimeRemovalInterval)
        {
            playerOvertime.Remainder = _settings.InitialOvertimeRemovalInterval;
        }
    }

    public TimeSpan GetTimeUntilNextRemoval(GameColor playerColor, OvertimeState state)
    {
        if (!state.PlayerOvertime.TryGetValue(playerColor, out var playerOvertime))
        {
            return _settings.InitialOvertimeRemovalInterval;
        }

        return _settings.InitialOvertimeRemovalInterval - playerOvertime.Remainder;
    }

    public bool HasEnteredOvertime(GameColor playerColor, OvertimeState state) =>
        state.PlayerOvertime.ContainsKey(playerColor);

    private NextOvertimeRemoval? PickNextRemoval(
        GameColor playerColor,
        IReadOnlyChessBoard board,
        PlayerOvertime playerOvertime
    )
    {
        var pickedNextRemoval = playerOvertime.PickedNextRemoval;
        if (
            pickedNextRemoval is not null
            && board.TryGetPieceAt(pickedNextRemoval.RemoveFrom, out var piece)
            && piece.Type == pickedNextRemoval.PieceType
            && piece.Color == pickedNextRemoval.PieceColor
        )
        {
            return pickedNextRemoval;
        }

        return PickRandomPiece(playerColor, board);
    }

    private NextOvertimeRemoval? PickRandomPiece(GameColor playerColor, IReadOnlyChessBoard board)
    {
        List<(AlgebraicPoint Position, Piece Occupant)> pieces =
        [
            .. board.EnumeratePieces().Where(x => x.Occupant.Color == playerColor),
        ];
        if (pieces.Count == 0)
        {
            return null;
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
        var piece = board.PeekPieceAt(removeFrom);
        if (piece is null)
        {
            return null;
        }

        return new(removeFrom, piece.Type, piece.Color);
    }
}
