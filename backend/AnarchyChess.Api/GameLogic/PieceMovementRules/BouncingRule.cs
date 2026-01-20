using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class BouncingRule(
    Offset initialOffset,
    Func<IReadOnlyChessBoard, Offset, IPieceMovementRule> ruleCreator,
    Func<IReadOnlyChessBoard, Move, bool> stopBouncingPredicate
) : IPieceMovementRule
{
    private readonly Offset _initialOffset = initialOffset;
    private readonly Func<IReadOnlyChessBoard, Offset, IPieceMovementRule> _ruleCreator =
        ruleCreator;
    private readonly Func<IReadOnlyChessBoard, Move, bool> _stopBouncingPredicate =
        stopBouncingPredicate;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        foreach (
            var move in FindBounces(
                board,
                position,
                position,
                movingPiece,
                visited: [position],
                intermediates: [],
                captured: [],
                _initialOffset
            )
        )
        {
            yield return move;
        }
    }

    private IEnumerable<Move> FindBounces(
        IReadOnlyChessBoard board,
        AlgebraicPoint originPosition,
        AlgebraicPoint currentPosition,
        Piece movingPiece,
        HashSet<AlgebraicPoint> visited,
        HashSet<IntermediateSquare> intermediates,
        List<MoveCapture> captured,
        Offset currentOffset
    )
    {
        var rule = _ruleCreator(board, currentOffset);
        Move? lastMove = null;
        foreach (var move in rule.Evaluate(board, currentPosition, movingPiece))
        {
            currentPosition = move.To;
            if (visited.Contains(currentPosition))
            {
                yield break;
            }
            visited.Add(currentPosition);

            lastMove = move with
            {
                From = originPosition,
                IntermediateSquares = intermediates,
                Captures = [.. captured, .. move.Captures],
            };
            yield return lastMove;
        }
        if (lastMove is null || _stopBouncingPredicate(board, lastMove))
        {
            yield break;
        }

        currentPosition = lastMove.To;
        if (currentPosition.X >= board.Width - 1 || currentPosition.X <= 0)
        {
            currentOffset = currentOffset with { X = currentOffset.X * -1 };
        }
        else if (currentPosition.Y >= board.Height - 1 || currentPosition.Y <= 0)
        {
            currentOffset = currentOffset with { Y = currentOffset.Y * -1 };
        }
        else
        {
            yield break;
        }

        IntermediateSquare intermediate = new(
            lastMove.To,
            IsCapture: lastMove.Captures.Count > captured.Count
        );
        foreach (
            var move in FindBounces(
                board,
                originPosition,
                currentPosition,
                movingPiece,
                visited: [.. visited],
                intermediates: [.. intermediates, intermediate],
                captured: [.. captured, .. lastMove.Captures],
                currentOffset
            )
        )
        {
            yield return move;
        }
    }
}
