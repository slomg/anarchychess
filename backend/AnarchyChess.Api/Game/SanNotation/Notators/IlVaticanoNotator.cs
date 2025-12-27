using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class IlVaticanoNotator(IPieceLetterMap pieceLetterMap) : BaseSanNotator(pieceLetterMap)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.IlVaticano;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        sb.Append("B-O-O-B");
}
