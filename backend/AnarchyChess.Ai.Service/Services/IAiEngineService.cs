using AnarchyChess.Ai.Service.DTO;
using ProtoBuf.Grpc.Configuration;

namespace AnarchyChess.Ai.Service.Services;

[Service]
public interface IAiEngineService
{
    [Operation]
    ValueTask<AiEngineMoveReply> FindBestMoveAsync(
        AiEngineMoveRequest request,
        CancellationToken token = default
    );

    [Operation]
    ValueTask<HealthReply> CheckHealthAsync(CancellationToken token = default);
}
