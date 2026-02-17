using System.ServiceModel;
using AnarchyChess.Ai.Service.DTO;

namespace AnarchyChess.Ai.Service.Services;

[ServiceContract]
public interface IAiEngineService
{
    ValueTask<AiEngineMoveReply?> FindBestMoveAsync(AiEngineMoveRequest request);
}
