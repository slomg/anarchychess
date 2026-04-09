using System.Text.Json;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IFenDecoder
{
    ErrorOr<ChessBoard> DecodeFen(string fen);
}

public class FenDecoder(IPieceLetterMap pieceLetterMap, IOptions<JsonOptions> jsonOptions)
    : IFenDecoder
{
    private readonly IPieceLetterMap _pieceLetterMap = pieceLetterMap;
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;

    /// <summary>
    /// Custom Fen:
    /// [pieces] [<see cref="FenParts"/> json]
    /// </summary>
    public ErrorOr<ChessBoard> DecodeFen(string fen)
    {
        if (string.IsNullOrEmpty(fen))
            return GameErrors.MalformedFenParts;

        var parts = fen.Split(" ");
        if (parts.Length > 2 || parts.Length < 1)
            return GameErrors.MalformedFenParts;

        string piecesPart = parts[0];
        var piecesResult = ParsePieces(piecesPart);
        if (piecesResult.IsError)
            return piecesResult.Errors;
        var (pieces, width, height) = piecesResult.Value;

        return parts.Length > 1
            ? ParseFenParts(parts[1], pieces, width, height)
            : new ChessBoard(pieces, height: height, width: width);
    }

    private ErrorOr<(Dictionary<AlgebraicPoint, Piece> pieces, int width, int height)> ParsePieces(
        string piecesPart
    )
    {
        Dictionary<AlgebraicPoint, Piece> pieces = [];

        var ranks = piecesPart.Split('/').Reverse().ToArray();
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
                return GameErrors.MalformedFenPieces;

            // width was already set, but the x indicates a different width
            if (width != 0 && x != width)
                return GameErrors.MalformedFenPieces;

            width = x;
        }

        return (pieces, width, height);
    }

    private static GameColor? GetColorFromLetter(char letter)
    {
        // not letter = neutral pieces
        if (!char.IsLetter(letter))
            return null;

        return char.IsUpper(letter) ? GameColor.White : GameColor.Black;
    }

    private ErrorOr<ChessBoard> ParseFenParts(
        string fenPartsString,
        Dictionary<AlgebraicPoint, Piece> pieces,
        int width,
        int height
    )
    {
        FenParts? fenParts;
        try
        {
            fenParts = JsonSerializer.Deserialize<FenParts>(fenPartsString, _jsonOptions);
        }
        catch
        {
            return GameErrors.MalformedFenParts;
        }

        if (fenParts is null)
        {
            return GameErrors.MalformedFenParts;
        }

        var sideToMove = ParseSideToMove(fenParts);

        var movedPiecesResult = ParseMovedPieces(fenParts, pieces, width, height);
        if (movedPiecesResult.IsError)
        {
            return movedPiecesResult.Errors;
        }

        var stunnedPiecesResult = ParseStunnedPieces(fenParts, width, height);
        if (stunnedPiecesResult.IsError)
        {
            return stunnedPiecesResult.Errors;
        }

        var lastMoveResult = ParseLastMove(fenParts, pieces, width, height);
        if (lastMoveResult.IsError)
        {
            return lastMoveResult.Errors;
        }
        var lastMove = lastMoveResult.Value;

        return new ChessBoard(
            pieces,
            width: width,
            height: height,
            sideToMove: sideToMove,
            moves: lastMove is null ? [] : [lastMove],
            halfMoveClock: fenParts.HalfMoveClock ?? 0,
            stunnedPieces: stunnedPiecesResult.Value
        );
    }

    private static GameColor ParseSideToMove(FenParts fenParts) =>
        fenParts.SideToMove ?? GameColor.White;

    private static ErrorOr<Dictionary<AlgebraicPoint, int>> ParseStunnedPieces(
        FenParts fenParts,
        int width,
        int height
    )
    {
        Dictionary<AlgebraicPoint, int> stunnedPieces = [];
        if (fenParts.StunnedPieces is null)
        {
            return stunnedPieces;
        }

        foreach (var (strPoint, turns) in fenParts.StunnedPieces)
        {
            if (
                !AlgebraicPoint.TryParse(
                    strPoint,
                    maxWidth: width,
                    maxHeight: height,
                    out var point
                )
            )
            {
                return GameErrors.MalformedFenStunnedPieces;
            }
            if (turns < 0)
            {
                return GameErrors.MalformedFenStunnedPieces;
            }

            stunnedPieces[point] = turns;
        }

        return stunnedPieces;
    }

    private static ErrorOr<Success> ParseMovedPieces(
        FenParts fenParts,
        Dictionary<AlgebraicPoint, Piece> pieces,
        int width,
        int height
    )
    {
        if (fenParts.MovedPieces is null)
        {
            return Result.Success;
        }

        foreach (var strPoint in fenParts.MovedPieces)
        {
            if (
                !AlgebraicPoint.TryParse(
                    strPoint,
                    maxWidth: width,
                    maxHeight: height,
                    out var point
                )
            )
            {
                return GameErrors.MalformedFenMovedPieces;
            }

            if (!pieces.TryGetValue(point, out var piece))
            {
                continue;
            }

            pieces[point] = piece with { HasMoved = true };
        }

        return Result.Success;
    }

    private static ErrorOr<Move?> ParseLastMove(
        FenParts fenParts,
        Dictionary<AlgebraicPoint, Piece> pieces,
        int width,
        int height
    )
    {
        if (fenParts.LastMove is null)
        {
            return (Move?)null;
        }
        var lastMove = fenParts.LastMove;

        if (
            !AlgebraicPoint.TryParse(
                lastMove.From,
                maxWidth: width,
                maxHeight: height,
                out var fromPoint
            )
        )
        {
            return GameErrors.MalformedFenLastMove;
        }

        if (
            !AlgebraicPoint.TryParse(
                lastMove.To,
                maxWidth: width,
                maxHeight: height,
                out var toPoint
            )
        )
        {
            return GameErrors.MalformedFenLastMove;
        }

        if (!pieces.TryGetValue(toPoint, out var piece))
        {
            return (Move?)null;
        }

        var captures = GetCaptures(lastMove, width, height);
        if (captures is null)
        {
            return GameErrors.MalformedFenLastMove;
        }

        pieces[toPoint] = piece;
        return new Move(fromPoint, toPoint, piece, captures: captures);
    }

    private static List<MoveCapture>? GetCaptures(FenLastMove move, int width, int height)
    {
        if (move.Captures is null)
            return [];

        List<MoveCapture> result = [];
        foreach (var capture in move.Captures)
        {
            if (
                !AlgebraicPoint.TryParse(
                    capture.Pos,
                    maxWidth: width,
                    maxHeight: height,
                    out var position
                )
            )
            {
                return null;
            }

            result.Add(new MoveCapture(capture.Piece, position));
        }
        return result;
    }
}
