using System.Text;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public interface ISanNotator
{
    SpecialMoveType HandlesMoveType { get; }
    void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb);
}
