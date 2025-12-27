using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IPieceToLetter
{
    char GetLetter(PieceType piece);
}

public class PieceToLetter : IPieceToLetter
{
    private readonly Dictionary<PieceType, char> _pieceToLetter = new()
    {
        [PieceType.King] = 'k',
        [PieceType.Queen] = 'q',
        [PieceType.Pawn] = 'p',
        [PieceType.UnderagePawn] = 'd',
        [PieceType.SterilePawn] = 's',
        [PieceType.Rook] = 'r',
        [PieceType.Bishop] = 'b',
        [PieceType.Horsey] = 'h',
        [PieceType.Knook] = 'n',
        [PieceType.Antiqueen] = 'a',
        [PieceType.Checker] = 'c',
        [PieceType.TraitorRook] = '+',
    };

    public string GetLetter(PieceType piece) => _pieceToLetterMap.GetValueOrDefault(piece, "?");
    public char GetLetter(PieceType piece) => _pieceToLetter.GetValueOrDefault(piece, '?');
}
