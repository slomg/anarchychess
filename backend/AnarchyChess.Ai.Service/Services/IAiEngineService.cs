using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using ProtoBuf.Grpc.Configuration;

namespace AnarchyChess.Ai.Service.Services;

[Service]
public interface IAiEngineService
{
    [Operation]
    ValueTask<MoveEvaluation> FindBestMoveAsync(
        AiEngineMoveRequest request,
        CancellationToken token = default
    );

    [Operation]
    ValueTask<EvaluateAllMovesReply> EvaluateAllMovesAsync(
        AiEngineMoveRequest request,
        CancellationToken token = default
    );

    [Operation]
    ValueTask<HealthReply> CheckHealthAsync(CancellationToken token = default);
}
