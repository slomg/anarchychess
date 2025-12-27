using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class VerticalCastleNotator(IPieceLetterMap pieceLetterMap) : BaseSanNotator(pieceLetterMap)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.VerticalCastle;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append("O-O-O-O-O-O");
        foreach (var capture in move.Captures)
        {
            sb.Append('x');
            sb.Append(capture.Position.AsAlgebraic());
        }
    }
}
