using System.Text;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.SanNotation;

public interface ISanCalculator
{
    string CalculateSan(Move move, IEnumerable<Move> legalMoves, bool isKingCapture = false);
}

public class SanCalculator : ISanCalculator
{
    private readonly IPieceLetterMap _pieceLetterMap;
    private readonly Dictionary<SpecialMoveType, ISanNotator> _notators;
    private readonly ISanNotator _defaultNotator;

    public SanCalculator(IPieceLetterMap pieceLetterMap, IEnumerable<ISanNotator> notators)
    {
        _pieceLetterMap = pieceLetterMap;
        _notators = notators.ToDictionary(x => x.HandlesMoveType);
        _defaultNotator = _notators[SpecialMoveType.None];
    }

    public string CalculateSan(Move move, IEnumerable<Move> legalMoves, bool isKingCapture = false)
    {
        var notator = _notators.GetValueOrDefault(move.SpecialMoveType, _defaultNotator);

        StringBuilder sb = new();
        notator.Notate(move, legalMoves, sb);

        if (move.PromotesTo is PieceType promotesTo)
        {
            sb.Append('=');
            sb.Append(char.ToUpper(_pieceLetterMap.GetLetter(promotesTo)));
        }

        if (isKingCapture)
            sb.Append('#');

        return sb.ToString();
    }
}
