using System.IO.Compression;
using System.Text.Json;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Game.Services;

public interface IMoveEncoder
{
    CompressedMoves EncodeMoves(IEnumerable<MovePath> moves);
}

public class MoveEncoder(IOptions<JsonOptions> jsonOptions) : IMoveEncoder
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;

    public CompressedMoves EncodeMoves(IEnumerable<MovePath> moves)
    {
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(moves, _jsonOptions);
        int maxCompressedLength = BrotliEncoder.GetMaxCompressedLength(jsonBytes.Length);
        byte[] compressed = new byte[maxCompressedLength];

        if (
            !BrotliEncoder.TryCompress(
                source: jsonBytes,
                destination: compressed,
                out int bytesWritten,
                quality: 1,
                window: 16
            )
        )
        {
            throw new InvalidOperationException("Brotli compression failed.");
        }

        if (bytesWritten == compressed.Length)
        {
            return Convert.ToBase64String(compressed);
        }

        byte[] result = new byte[bytesWritten];
        Array.Copy(compressed, result, bytesWritten);
        return Convert.ToBase64String(result);
    }
}
