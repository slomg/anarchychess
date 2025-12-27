using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class OmnipotentPawnNotator(IPieceLetterMap pieceLetterMap) : BaseSanNotator(pieceLetterMap)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.OmnipotentPawnSpawn;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        NotateDestination(move, sb);
}
