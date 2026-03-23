using AnarchyChess.Api.GameLogic.ForeverRules;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceDefinitions;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic;

public interface ILegalMoveCalculator
{
    IEnumerable<Move> CalculateAllLegalMoves(IReadOnlyChessBoard board);
    IEnumerable<Move> CalculateForeverRules(IReadOnlyChessBoard board);
    IEnumerable<Move> CalculateLegalMovesForPiece(
        IReadOnlyChessBoard board,
        AlgebraicPoint position
    );
}

public class LegalMoveCalculator : ILegalMoveCalculator
{
    private readonly Dictionary<PieceType, IPieceDefinition> _pieceDefinitions = [];
    private readonly IEnumerable<IForeveRule> _foreverRules;

    public LegalMoveCalculator(
        IEnumerable<IPieceDefinition> pieceDefinitions,
        IEnumerable<IForeveRule> foreverRules
    )
    {
        _pieceDefinitions = pieceDefinitions.ToDictionary(definition => definition.Type);
        if (_pieceDefinitions.Count != Enum.GetNames<PieceType>().Length)
            throw new InvalidOperationException("Could not find definitions for all pieces");

        _foreverRules = foreverRules;
    }

    public IEnumerable<Move> CalculateAllLegalMoves(IReadOnlyChessBoard board)
    {
        foreach (var (position, piece) in board.EnumeratePieces())
        {
            foreach (var move in CalculateLegalMovesForPiece(board, position))
            {
                yield return move;
            }
        }

        foreach (var move in CalculateForeverRules(board))
        {
            yield return move;
        }
    }

    public IEnumerable<Move> CalculateLegalMovesForPiece(
        IReadOnlyChessBoard board,
        AlgebraicPoint position
    )
    {
        if (!board.TryGetPieceAt(position, out var piece))
        {
            yield break;
        }

        if (piece.StunnedForTurns > 0)
        {
            yield break;
        }

        var isColorMismatch = piece.Color is not null && piece.Color != board.SideToMove;
        if (isColorMismatch)
        {
            yield break;
        }

        var pieceBehaviours = _pieceDefinitions[piece.Type].GetBehaviours(board, position, piece);
        foreach (var behaviour in pieceBehaviours)
        {
            foreach (var move in behaviour.Evaluate(board, position, piece))
            {
                yield return move;
            }
        }
    }

    public IEnumerable<Move> CalculateForeverRules(IReadOnlyChessBoard board)
    {
        foreach (var rule in _foreverRules)
        {
            foreach (var move in rule.GetBehaviours(board, board.SideToMove))
            {
                yield return move;
            }
        }
    }
}
