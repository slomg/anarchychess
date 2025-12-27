using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class BetaDecayNotator(IPieceLetterMap pieceLetterMap) : BaseSanNotator(pieceLetterMap)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.RadioactiveBetaDecay;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append(PieceChar(move.Piece.Type));
        sb.Append('β');

        foreach (var spawn in move.PieceSpawns)
        {
            sb.Append(PieceChar(spawn.Type));
            sb.Append(spawn.Position.AsAlgebraic());
        }
    }
}
