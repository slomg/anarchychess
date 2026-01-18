using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IPlayableMoveProvider
{
    LegalMoveSet CalculateAllPlayableMoves(IReadOnlyChessBoard board);
    Move? GetPieceMoveByKey(
        IReadOnlyChessBoard board,
        AlgebraicPoint piecePosition,
        MoveKey moveKey
    );
}

public class PlayableMoveProvider(
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder moveEncoder
) : IPlayableMoveProvider
{
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = moveEncoder;

    public LegalMoveSet CalculateAllPlayableMoves(IReadOnlyChessBoard board)
    {
        if (!BothSidesHaveKing(board))
            return new();

        var allMoves = _legalMoveCalculator.CalculateAllLegalMoves(board).ToList();
        var maxPriority =
            allMoves.Count != 0 ? allMoves.Max(m => m.ForcedPriority) : ForcedMovePriority.None;
        var legalMoves = allMoves.Where(m => m.ForcedPriority == maxPriority).ToList();

        Dictionary<MoveKey, Move> moveMap = [];
        List<MovePath> movePaths = [];
        foreach (var move in legalMoves)
        {
            MoveKey key = new(move);

            movePaths.Add(MovePath.FromMove(move, board.Width, moveKey: key));
            moveMap[key] = move;
        }

        var encodedMoves = _moveEncoder.EncodeMoves(movePaths);
        return new(
            MoveMap: moveMap,
            MovePaths: movePaths,
            EncodedMoves: encodedMoves,
            HasForcedMoves: maxPriority > ForcedMovePriority.None
        );
    }

    public Move? GetPieceMoveByKey(
        IReadOnlyChessBoard board,
        AlgebraicPoint piecePosition,
        MoveKey moveKey
    )
    {
        if (!BothSidesHaveKing(board))
            return null;

        var pieceLegalMove = _legalMoveCalculator
            .CalculateLegalMovesForPiece(board, piecePosition)
            .FirstOrDefault(x => new MoveKey(x) == moveKey);
        if (pieceLegalMove is not null)
            return pieceLegalMove;

        return _legalMoveCalculator
            .CalculateForeverRules(board)
            .FirstOrDefault(x => new MoveKey(x) == moveKey);
    }

    private static bool BothSidesHaveKing(IReadOnlyChessBoard board) =>
        board.HasPieceWith(PieceType.King, GameColor.White)
        && board.HasPieceWith(PieceType.King, GameColor.Black);
}
