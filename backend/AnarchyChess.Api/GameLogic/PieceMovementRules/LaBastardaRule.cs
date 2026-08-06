using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class LaBastardaRule(IPieceMovementRule movementRule) : IPieceMovementRule
{
    private readonly IPieceMovementRule _movementRule = movementRule;

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
        foreach (var move in _movementRule.Evaluate(board, position, movingPiece))
        {
            var oppositeVector = (move.To - move.From) * -1;
            var oppositeColor = movingPiece.Color.Value.Invert();
            var isEscapingQueen = false;

            isEscapingQueen |= isValidOpponentQueen(
                board,
                move.From + oppositeVector,
                oppositeColor
            );
            if (oppositeVector.X != 0 && oppositeVector.Y != 0)
            {
                isEscapingQueen |= isValidOpponentQueen(
                    board,
                    move.From + new Offset(oppositeVector.X, 0),
                    oppositeColor
                );
                isEscapingQueen |= isValidOpponentQueen(
                    board,
                    move.From + new Offset(0, oppositeVector.Y),
                    oppositeColor
                );
                isEscapingQueen |= isValidOpponentQueen(
                    board,
                    move.From + new Offset(-oppositeVector.X, oppositeVector.Y),
                    oppositeColor
                );
                isEscapingQueen |= isValidOpponentQueen(
                    board,
                    move.From + new Offset(oppositeVector.X, -oppositeVector.Y),
                    oppositeColor
                );
            }
            else
            {
                isEscapingQueen |= isValidOpponentQueen(
                    board,
                    move.From + oppositeVector + new Offset(oppositeVector.Y, oppositeVector.X),
                    oppositeColor
                );
                isEscapingQueen |= isValidOpponentQueen(
                    board,
                    move.From + oppositeVector - new Offset(oppositeVector.Y, oppositeVector.X),
                    oppositeColor
                );
            }

            if (isEscapingQueen)
            {
                List<PieceSpawn> spawns = [];
                spawns.Add(
                    new PieceSpawn(
                        Type: PieceType.UnderagePawn,
                        Color: oppositeColor,
                        Position: position
                    )
                );
                yield return move with
                {
                    PieceSpawns = spawns,
                    SpecialMoveType = SpecialMoveType.LaBastarda,
                };
            }
            else
            {
                yield return move;
            }
        }
    }

    private bool isValidOpponentQueen(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        GameColor color
    )
    {
        return board.TryGetPieceAt(position, out var potentialQueen)
            && potentialQueen.Type == PieceType.Queen
            && potentialQueen.Color == color
            && !board.StunnedPieces.ContainsKey(position);
    }
}
