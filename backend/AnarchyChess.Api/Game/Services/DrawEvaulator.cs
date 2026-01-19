using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IDrawEvaulator
{
    void RegisterInitialPosition(FenNotation fen, AutoDrawState state);
    bool TryEvaluateDraw(
        Move move,
        FenNotation fen,
        IReadOnlyChessBoard board,
        AutoDrawState state,
        [NotNullWhen(true)] out GameEndStatus? endStatus
    );
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.AutoDrawState")]
public class AutoDrawState
{
    [Id(0)]
    public Dictionary<string, int> FenOccurrences { get; init; } = [];
}

public class DrawEvaulator(IGameResultDescriber gameResultDescriber) : IDrawEvaulator
{
    private readonly IGameResultDescriber _gameResultDescriber = gameResultDescriber;

    public void RegisterInitialPosition(FenNotation fen, AutoDrawState state) =>
        state.FenOccurrences.TryAdd(fen.Position, 1);

    public bool TryEvaluateDraw(
        Move move,
        FenNotation fen,
        IReadOnlyChessBoard board,
        AutoDrawState state,
        [NotNullWhen(true)] out GameEndStatus? endStatus
    )
    {
        if (IsThreeFold(fen, state))
        {
            endStatus = _gameResultDescriber.ThreeFold();
            return true;
        }
        if (Is50Moves(board))
        {
            endStatus = _gameResultDescriber.FiftyMoves();
            return true;
        }
        if (IsKingTouch(move, board))
        {
            endStatus = _gameResultDescriber.KingTouch();
            return true;
        }

        endStatus = null;
        return false;
    }

    private static bool IsThreeFold(FenNotation fen, AutoDrawState state)
    {
        if (state.FenOccurrences.TryAdd(fen.Position, 1))
            return false;

        state.FenOccurrences[fen.Position]++;
        return state.FenOccurrences[fen.Position] >= 3;
    }

    private static bool Is50Moves(IReadOnlyChessBoard board) => board.HalfMoveClock >= 100;

    private static bool IsKingTouch(Move move, IReadOnlyChessBoard board)
    {
        if (move.Piece.Type is not PieceType.King)
            return false;

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (y == 0 && x == 0)
                {
                    continue;
                }

                var position = move.To - new Offset(x, y);
                if (
                    board.TryGetPieceAt(position, out var touchingPiece)
                    && touchingPiece.Type is PieceType.King
                    && touchingPiece.Color != move.Piece.Color
                )
                {
                    return true;
                }
            }
        }

        return false;
    }
}
