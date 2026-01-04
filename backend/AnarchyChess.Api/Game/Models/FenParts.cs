using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

public record FenParts(
    GameColor? SideToMove,
    List<AlgebraicString>? MovedPieces,
    FenLastMove? LastMove,
    int? HalfMoveClock
);

public record FenLastMove(AlgebraicString From, AlgebraicString To)
{
    public static FenLastMove? FromMove(Move? move)
    {
        if (move is null)
            return null;
        return new(move.From.AsAlgebraic(), move.To.AsAlgebraic());
    }
}
