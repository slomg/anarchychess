using AnarchyChess.EngineShared;

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

    private readonly Dictionary<char, PieceType?> _letterToPiece = new()
    {
        ['k'] = PieceType.King,
        ['q'] = PieceType.Queen,
        ['p'] = PieceType.Pawn,
        ['d'] = PieceType.UnderagePawn,
        ['s'] = PieceType.SterilePawn,
        ['r'] = PieceType.Rook,
        ['b'] = PieceType.Bishop,
        ['h'] = PieceType.Horsey,
        ['n'] = PieceType.Knook,
        ['a'] = PieceType.Antiqueen,
        ['c'] = PieceType.Checker,
        ['+'] = PieceType.TraitorRook,
    };

    public char GetLetter(PieceType piece) => _pieceToLetter.GetValueOrDefault(piece, '?');

    public PieceType? GetPiece(char letter) =>
        _letterToPiece.GetValueOrDefault(char.ToLower(letter));
}
