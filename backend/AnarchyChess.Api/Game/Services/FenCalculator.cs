using System.Text;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using ErrorOr;

namespace AnarchyChess.Api.Game.Services;

public interface IFenCalculator
{
    string CalculateFen(IReadOnlyChessBoard board);
    ErrorOr<ChessBoard> DecodeFen(string fen, GameColor sideToMove = GameColor.White);
}

public class FenCalculator(IPieceLetterMap pieceLetterMap) : IFenCalculator
{
    private readonly IPieceLetterMap _pieceLetterMap = pieceLetterMap;

    public string CalculateFen(IReadOnlyChessBoard board)
    {
        StringBuilder sb = new();

        // enumerate from black perspective because we FENs start with the black pieces
        for (int y = board.Height - 1; y >= 0; y--)
        {
            int emptyCount = 0;
            for (int x = 0; x < board.Width; x++)
            {
                AlgebraicPoint point = new(x, y);
                if (!board.TryGetPieceAt(point, out var piece))
                {
                    emptyCount++;
                    continue;
                }

                if (emptyCount > 0)
                {
                    sb.Append(emptyCount);
                    emptyCount = 0;
                }

                var pieceLetter = _pieceLetterMap.GetLetter(piece.Type);
                pieceLetter = piece.Color.Match(
                    whenWhite: char.ToUpper(pieceLetter),
                    whenBlack: char.ToLower(pieceLetter),
                    whenNeutral: pieceLetter
                );

                sb.Append(pieceLetter);
            }

            if (emptyCount > 0)
                sb.Append(emptyCount);
            if (y > 0)
                sb.Append('/');
        }

        var fen = sb.ToString();
        return fen;
    }

    public ErrorOr<ChessBoard> DecodeFen(string fen, GameColor sideToMove = GameColor.White)
    {
        if (fen.Length == 0)
            return GameErrors.MalformedFen;

        Dictionary<AlgebraicPoint, Piece> pieces = [];

        var ranks = fen.Split('/').Reverse().ToArray();
        int height = ranks.Length;
        int width = 0;
        for (var y = 0; y < ranks.Length; y++)
        {
            var rank = ranks[y];

            string num = "";
            int x = 0;
            foreach (var square in rank)
            {
                if (char.IsDigit(square))
                {
                    num += square;
                    continue;
                }

                if (num.Length > 0)
                {
                    x += int.Parse(num);
                    num = "";
                }

                var color = GetColorFromLetter(square);
                var pieceType = _pieceLetterMap.GetPiece(square);
                if (pieceType is null)
                    return GameErrors.InvalidPieceLetter;

                Piece piece = new(pieceType.Value, color);
                pieces[new AlgebraicPoint(x, y)] = piece;

                x++;
            }

            // handle trailing numbers in the rank
            if (num.Length > 0)
            {
                x += int.Parse(num);
            }

            if (x == 0)
                return GameErrors.MalformedFen;

            // width was already set, but the x indicates a different width
            if (width != 0 && x != width)
                return GameErrors.MalformedFen;

            width = x;
        }

        return new ChessBoard(pieces, height: height, width: width, sideToMove);
    }

    private static GameColor? GetColorFromLetter(char letter)
    {
        bool isLetter = (letter >= 'A' && letter <= 'Z') || (letter >= 'a' && letter <= 'z');
        // not letter = neutral pieces
        if (!isLetter)
            return null;

        return char.IsUpper(letter) ? GameColor.White : GameColor.Black;
    }
}
