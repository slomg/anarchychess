using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.Models;

public record FenParts(
    GameColor? SideToMove,
    IReadOnlyCollection<AlgebraicString>? MovedPieces,
    IReadOnlyDictionary<string, int>? StunnedPieces,
    FenLastMove? LastMove,
    int? HalfMoveClock
);

public record FenLastMove(
    AlgebraicString From,
    AlgebraicString To,
    IReadOnlyList<FenCapture>? Captures
)
{
    public static FenLastMove? FromMove(Move? move)
    {
        if (move is null)
            return null;

        return new(
            move.From.AsAlgebraic(),
            move.To.AsAlgebraic(),
            Captures: move.Captures.Count > 0
                ? [.. move.Captures.Select(FenCapture.FromCapture)]
                : null
        );
    }
}

public record FenCapture(Piece Piece, AlgebraicString Pos)
{
    public static FenCapture FromCapture(MoveCapture capture)
    {
        return new(capture.CapturedPiece, capture.Position.AsAlgebraic());
    }
}
