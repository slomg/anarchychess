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
    private readonly IPieceToLetter _pieceToLetter;
    private readonly Dictionary<SpecialMoveType, ISanNotator> _notators;
    private readonly ISanNotator _defaultNotator;

    public SanCalculator(IPieceToLetter pieceToLetter, IEnumerable<ISanNotator> notators)
    {
        _pieceToLetter = pieceToLetter;
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
            sb.Append(char.ToUpper(_pieceToLetter.GetLetter(promotesTo)));
        }

        if (isKingCapture)
            sb.Append('#');

        return sb.ToString();
    }
}
