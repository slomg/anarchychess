using System.Text;
using System.Text.Json;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IFenEncoder
{
    FenNotation EncodeFen(IReadOnlyChessBoard board);
}

public record FenNotation(string Position, string FullFen);

public class FenEncoder(IPieceLetterMap pieceLetterMap, IOptions<JsonOptions> jsonOptions)
    : IFenEncoder
{
    private readonly IPieceLetterMap _pieceLetterMap = pieceLetterMap;
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;

    /// <summary>
    /// Custom Fen:
    /// [pieces] [<see cref="FenParts"/> json]
    /// </summary>
    public FenNotation EncodeFen(IReadOnlyChessBoard board)
    {
        StringBuilder sb = new();
        AddPieces(board, sb);
        var fenPieces = sb.ToString();

        FenParts parts = new(
            SideToMove: board.SideToMove is GameColor.Black ? GameColor.Black : null,
            MovedPieces: GetMovedPieces(board),
            StunnedPieces: board.StunnedPieces.Count > 0
                ? board.StunnedPieces.ToDictionary(kvp => kvp.Key.AsAlgebraic(), kvp => kvp.Value)
                : null,
            LastMove: FenLastMove.FromMove(board.Moves.Count > 0 ? board.Moves[^1] : null),
            HalfMoveClock: board.HalfMoveClock > 0 ? board.HalfMoveClock : null
        );
        var serialized = JsonSerializer.Serialize(parts, _jsonOptions);
        if (serialized != "{}")
        {
            sb.Append(' ');
            sb.Append(serialized);
        }

        return new(Position: fenPieces, FullFen: sb.ToString());
    }

    private void AddPieces(IReadOnlyChessBoard board, StringBuilder sb)
    {
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
    }

    private static List<AlgebraicString>? GetMovedPieces(IReadOnlyChessBoard board)
    {
        List<AlgebraicString> result = [];
        GameColor[] colors = [GameColor.White, GameColor.Black];
        foreach (var pieceType in GameConstants.PiecesTrackingHasMoved)
        {
            foreach (var color in colors)
            {
                foreach (var (piece, position) in board.GetAllPiecesWith(pieceType, color))
                {
                    if (piece.HasMoved)
                    {
                        result.Add(position.AsAlgebraic());
                    }
                }
            }
        }

        return result.Count > 0 ? result : null;
    }
}
