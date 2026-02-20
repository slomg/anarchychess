using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.AnarchyBot.Services;

public interface IAnarchyBotService
{
    Task<bool> CheckHealthAsync(CancellationToken token = default);
    Task<AiEngineMoveReply> FindBestMoveAsync(
        IReadOnlyChessBoard board,
        CancellationToken token = default
    );
}

public class AnarchyBotService(ILogger<AnarchyBotService> logger, IAiEngineService aiEngineService)
    : IAnarchyBotService
{
    private readonly ILogger<AnarchyBotService> _logger = logger;
    private readonly IAiEngineService _aiEngineService = aiEngineService;

    public async Task<AiEngineMoveReply> FindBestMoveAsync(
        IReadOnlyChessBoard board,
        CancellationToken token = default
    )
    {
        PrevMoveStateDto? prevMove = GetPrevMoveState(board);
        AiEngineMoveRequest request = new(
            Pieces: board.EnumeratePieces().ToDictionary(),
            IsWhiteToMove: board.SideToMove is GameColor.White,
            prevMove
        );

        var bestMove = await _aiEngineService.FindBestMoveAsync(request, token);
        return bestMove;
    }

    public async Task<bool> CheckHealthAsync(CancellationToken token = default)
    {
        try
        {
            var result = await _aiEngineService.CheckHealthAsync(token);
            return result.IsHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Exception when checking ai engine service health: {Ex}", ex);
            return false;
        }
    }

    private static PrevMoveStateDto? GetPrevMoveState(IReadOnlyChessBoard board)
    {
        if (board.Moves.Count == 0)
        {
            return null;
        }

        Move lastMove = board.Moves[^1];
        return new(
            From: lastMove.From,
            To: lastMove.To,
            Piece: lastMove.Piece,
            Captures: [.. lastMove.Captures.Select(x => x.Position)]
        );
    }
}
