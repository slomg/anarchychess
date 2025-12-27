using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IPieceLetterMap
{
    char GetLetter(PieceType piece);
    PieceType? GetPiece(char letter);
}

public class PieceLetterMap : IPieceLetterMap
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

}
