using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.Bots.Errors;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using ErrorOr;
using Grpc.Core;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotService
{
    Task<bool> CheckHealthAsync(CancellationToken token = default);
    Task<ErrorOr<MoveEvaluation[]>> EvaluateAllMovesAsync(
        IReadOnlyChessBoard board,
        int depth,
        CancellationToken token = default
    );
    Task<ErrorOr<MoveEvaluation>> FindBestMoveAsync(
        IReadOnlyChessBoard board,
        int depth,
        CancellationToken token = default
    );
}

public class BotService(ILogger<BotService> logger, IAiEngineService aiEngineService) : IBotService
{
    private readonly ILogger<BotService> _logger = logger;
    private readonly IAiEngineService _aiEngineService = aiEngineService;

    public async Task<ErrorOr<MoveEvaluation>> FindBestMoveAsync(
        IReadOnlyChessBoard board,
        int depth,
        CancellationToken token = default
    )
    {
        PrevMoveStateDto? prevMove = GetPrevMoveState(board);
        AiEngineMoveRequest request = new(
            Pieces: board.EnumeratePieces().ToDictionary(),
            IsWhiteToMove: board.SideToMove is GameColor.White,
            prevMove,
            Depth: depth
        );

        MoveEvaluation bestMove;
        try
        {
            bestMove = await _aiEngineService.FindBestMoveAsync(request, token);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning("Error when trying to get best bot move: {Ex}", ex);
            if (ex.StatusCode is StatusCode.Unavailable)
            {
                return BotErrors.BotOffline;
            }
            else if (ex.StatusCode is StatusCode.InvalidArgument)
            {
                return BotErrors.NoMoveFound;
            }
            else
            {
                return BotErrors.BotFailure;
            }
        }
        return bestMove;
    }

    public async Task<ErrorOr<MoveEvaluation[]>> EvaluateAllMovesAsync(
        IReadOnlyChessBoard board,
        int depth,
        CancellationToken token = default
    )
    {
        PrevMoveStateDto? prevMove = GetPrevMoveState(board);
        AiEngineMoveRequest request = new(
            Pieces: board.EnumeratePieces().ToDictionary(),
            IsWhiteToMove: board.SideToMove is GameColor.White,
            prevMove,
            Depth: depth
        );

        EvaluateAllMovesReply reply;
        try
        {
            reply = await _aiEngineService.EvaluateAllMovesAsync(request, token);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning("Error when trying to evaluate all bot moves: {Ex}", ex);
            if (ex.StatusCode is StatusCode.Unavailable)
            {
                return BotErrors.BotOffline;
            }
            else if (ex.StatusCode is StatusCode.InvalidArgument)
            {
                return BotErrors.NoMoveFound;
            }
            else
            {
                return BotErrors.BotFailure;
            }
        }
        return reply.Moves;
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
