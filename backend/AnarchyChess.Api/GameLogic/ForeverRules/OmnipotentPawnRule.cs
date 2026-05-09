using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.GameLogic.ForeverRules;

public class OmnipotentPawnRule : IForeveRule
{
    public IEnumerable<Move> GetBehaviours(IReadOnlyChessBoard board, GameColor movingPlayer)
    {
        if (board.Moves.Count == 0)
            yield break;
        var lastMove = board.Moves[^1];

        var spawnPosition = movingPlayer.Match(
            whenWhite: GameLogicConstants.WhiteOmnipotentPawnSquare,
            whenBlack: GameLogicConstants.BlackOmnipotentPawnSquare
        );
        if (
            lastMove.To != spawnPosition
            || !lastMove.Captures.Any(capture => capture.CapturedPiece.Color == movingPlayer)
        )
            yield break;

        yield return new Move(
            from: lastMove.To,
            to: lastMove.To,
            piece: new Piece(PieceType.Pawn, movingPlayer),
            captures: [new MoveCapture(lastMove.To, board)],
            pieceSpawns: [new PieceSpawn(PieceType.Pawn, Color: movingPlayer, lastMove.To)],
            specialMoveType: SpecialMoveType.OmnipotentPawnSpawn,
            emphasizeSquare: true
        );
    }
}
