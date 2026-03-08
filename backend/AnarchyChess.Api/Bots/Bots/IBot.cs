using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.Profile.Models;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Bots;

public interface IBot
{
    UserId Id { get; }
    Task<ErrorOr<AiEngineMove>> FindMoveAsync(
        IReadOnlyChessBoard board,
        CancellationToken token = default
    );
}
