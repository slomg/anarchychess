using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public readonly record struct Throw(AlgebraicPoint From, AlgebraicPoint To);

public sealed class ThrowingRule : IPieceMovementRule
{
    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        if (movingPiece.Color is null)
        {
            yield break;
        }

        int forwardDirection = movingPiece.Color.Value.Match(whenWhite: 1, whenBlack: -1);
        Throw[] points =
        [
            .. ThrowLeft(board, position, movingPiece, forwardDirection),
            .. ThrowRight(board, position, movingPiece, forwardDirection),
            .. ThrowForward(board, position, movingPiece, forwardDirection),
        ];
        Dictionary<AlgebraicPoint, AlgebraicPoint[]> destinationToOrigins = points
            .GroupBy(x => x.To)
            .ToDictionary(x => x.Key, x => x.Select(t => t.From).ToArray());

        foreach (var (to, origins) in destinationToOrigins)
        {
            var stunnedPiece = board.PeekPieceAt(to);
            if (stunnedPiece?.Color == movingPiece.Color)
            {
                continue;
            }

            yield return new Move(
                from: position,
                to: to,
                movingPiece,
                specialMoveType: SpecialMoveType.Throw,
                // remove self if we hit a piece
                captures: stunnedPiece is null
                    ? null
                    : [new MoveCapture(CapturedPiece: movingPiece, Position: position)],
                stuns: stunnedPiece is null
                    ? null
                    : [new MoveStun(Position: to, Piece: stunnedPiece, StunForTurns: 1)],
                triggerSquares: origins
            );
        }
    }

    private static IEnumerable<Throw> ThrowRight(
        IReadOnlyChessBoard board,
        AlgebraicPoint origin,
        Piece movingPiece,
        int forwardDirection
    )
    {
        foreach (
            var point in Throw(
                board,
                origin,
                movingPiece,
                throwFrom:
                [
                    origin,
                    origin + new Offset(X: 0, Y: 1),
                    origin + new Offset(X: 0, Y: -1),
                ],
                direction: new Offset(X: 1, Y: forwardDirection)
            )
        )
        {
            yield return point;
        }
    }

    private static IEnumerable<Throw> ThrowLeft(
        IReadOnlyChessBoard board,
        AlgebraicPoint origin,
        Piece movingPiece,
        int forwardDirection
    )
    {
        foreach (
            var point in Throw(
                board,
                origin,
                movingPiece,
                throwFrom:
                [
                    origin,
                    origin + new Offset(X: 0, Y: 1),
                    origin + new Offset(X: 0, Y: -1),
                ],
                direction: new Offset(X: -1, Y: forwardDirection)
            )
        )
        {
            yield return point;
        }
    }

    private static IEnumerable<Throw> ThrowForward(
        IReadOnlyChessBoard board,
        AlgebraicPoint origin,
        Piece movingPiece,
        int forwardDirection
    )
    {
        foreach (
            var point in Throw(
                board,
                origin,
                movingPiece,
                throwFrom:
                [
                    origin,
                    origin + new Offset(X: 1, Y: 0),
                    origin + new Offset(X: -1, Y: 0),
                ],
                direction: new Offset(X: 0, Y: forwardDirection)
            )
        )
        {
            yield return point;
        }
    }

    private static IEnumerable<Throw> Throw(
        IReadOnlyChessBoard board,
        AlgebraicPoint origin,
        Piece movingPiece,
        AlgebraicPoint[] throwFrom,
        Offset direction
    )
    {
        AlgebraicPoint throwingPiecePosition = origin - direction;
        if (!board.TryGetPieceAt(throwingPiecePosition, out var throwingPiece))
        {
            yield break;
        }

        if (throwingPiece.Color != movingPiece.Color)
        {
            yield break;
        }

        if (
            MaterialValue.GetPieceValue(throwingPiece.Type)
                - MaterialValue.GetPieceValue(movingPiece.Type)
            < 100
        )
        {
            yield break;
        }

        foreach (AlgebraicPoint start in throwFrom)
        {
            AlgebraicPoint current = start;

            current += direction;
            while (board.IsWithinBoundaries(current))
            {
                yield return new(From: throwingPiecePosition, To: current);
                current += direction;
            }
        }
    }
}
