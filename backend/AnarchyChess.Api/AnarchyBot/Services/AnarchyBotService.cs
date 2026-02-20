using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.AnarchyBot.Services;

public interface IAnarchyBotService
{
    Task<AiEngineMoveReply> FindBestMoveAsync(IReadOnlyChessBoard board);
}

public class AnarchyBotService(IAiEngineService aiEngineService) : IAnarchyBotService
{
    private readonly IAiEngineService _aiEngineService = aiEngineService;

    public async Task<AiEngineMoveReply> FindBestMoveAsync(IReadOnlyChessBoard board)
    {
        PrevMoveStateDto? prevMove = GetPrevMoveState(board);
        AiEngineMoveRequest request = new(
            Pieces: board.EnumeratePieces().ToDictionary(),
            IsWhiteToMove: board.SideToMove is GameColor.White,
            prevMove
        );

        var bestMove = await _aiEngineService.FindBestMoveAsync(request);
        return bestMove;
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
