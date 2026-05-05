using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public sealed class QueentumTunnelNotator(IPieceLetterMap pieceLetterMap)
    : BaseSanNotator(pieceLetterMap)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.QueentumTunnel;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        AlgebraicPoint queenPosition = move.Piece.Type is PieceType.Queen ? move.From : move.To;
        AlgebraicPoint antiqueenPosition =
            move.Piece.Type is PieceType.Antiqueen ? move.From : move.To;

        sb.Append(PieceChar(PieceType.Queen));
        List<Move> ambiguousQueen =
        [
            .. legalMoves.Where(x =>
                x.SpecialMoveType is SpecialMoveType.QueentumTunnel
                && x.Piece.Type == PieceType.Queen
                && x.From != queenPosition
            ),
        ];
        (bool isQueenRankAmbiguous, bool isQueenFileAmbiguous) = DisambiguatePosition(
            queenPosition,
            ambiguousQueen,
            sb
        );
        if (!isQueenRankAmbiguous && !isQueenFileAmbiguous && ambiguousQueen.Count > 0)
        {
            sb.Append(FileLetter(queenPosition.X));
        }

        sb.Append('ψ');

        sb.Append(PieceChar(PieceType.Antiqueen));
        List<Move> ambiguousAntiqueen =
        [
            .. legalMoves.Where(x =>
                x.SpecialMoveType is SpecialMoveType.QueentumTunnel
                && x.Piece.Type == PieceType.Antiqueen
                && x.From != antiqueenPosition
            ),
        ];
        (bool isAntiqueenRankAmbiguous, bool isAntiqueenFileAmbiguous) = DisambiguatePosition(
            antiqueenPosition,
            ambiguousAntiqueen,
            sb
        );
        if (!isAntiqueenRankAmbiguous && !isAntiqueenFileAmbiguous && ambiguousAntiqueen.Count > 0)
        {
            sb.Append(FileLetter(antiqueenPosition.X));
        }
    }
}
