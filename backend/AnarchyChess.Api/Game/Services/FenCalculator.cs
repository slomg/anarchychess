using System.Text;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IFenCalculator
{
    string CalculateFen(IReadOnlyChessBoard board);
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
                var point = new AlgebraicPoint(x, y);
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
}
