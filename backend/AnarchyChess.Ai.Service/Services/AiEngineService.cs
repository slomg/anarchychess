using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using Grpc.Core;

namespace AnarchyChess.Ai.Service.Services;

public class AiEngineService(IAiEngine aiEngine) : IAiEngineService
{
    private readonly IAiEngine _aiEngine = aiEngine;

    public ValueTask<MoveEvaluation> FindBestMoveAsync(
        AiEngineMoveRequest request,
        CancellationToken token = default
    )
    {
        BitBoard board = BitBoard.FromPieces(
            request.Pieces,
            isWhiteToMove: request.IsWhiteToMove,
            prevMoveState: request.PrevMoveState
        );
        (BitMove? bestMove, int evalForBot) = _aiEngine.FindBestMove(board, depth: request.Depth);
        if (bestMove is null)
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "The provided position contains no legal moves and is invalid"
                )
            );
        }

        return ValueTask.FromResult(new MoveEvaluation(bestMove.Value, evalForBot));
    }

    public ValueTask<EvaluateAllMovesReply> EvaluateAllMovesAsync(
        AiEngineMoveRequest request,
        CancellationToken token = default
    )
    {
        BitBoard board = BitBoard.FromPieces(
            request.Pieces,
            isWhiteToMove: request.IsWhiteToMove,
            prevMoveState: request.PrevMoveState
        );
        MoveEvaluation[] moves = _aiEngine.EvaluateAllMoves(board, depth: request.Depth);
        if (moves.Length == 0)
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "The provided position contains no legal moves and is invalid"
                )
            );
        }

        return ValueTask.FromResult(new EvaluateAllMovesReply(moves));
    }

    public ValueTask<HealthReply> CheckHealthAsync(CancellationToken token = default) =>
        ValueTask.FromResult(new HealthReply(IsHealthy: true));
}
