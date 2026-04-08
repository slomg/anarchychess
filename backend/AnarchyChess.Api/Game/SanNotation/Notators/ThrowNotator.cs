using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class ThrowNotator(IPieceLetterMap pieceLetterMap) : BaseSanNotator(pieceLetterMap)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.Throw;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append(move.From.AsAlgebraic());
        sb.Append("->");
        sb.Append(move.To.AsAlgebraic());

        if (move.Stuns.Count > 0)
        {
            sb.Append('*');
        }
    }
}
