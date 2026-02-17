using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Service.Services;

public class AiEngineService(IAiEngine aiEngine) : IAiEngineService
{
    private readonly IAiEngine _aiEngine = aiEngine;

    public const int Depth = 8;

    public ValueTask<AiEngineMoveReply?> FindBestMoveAsync(AiEngineMoveRequest request)
    {
        BitBoard board = BitBoard.FromPieces(
            request.Pieces,
            isWhiteToMove: request.IsWhiteToMove,
            lastMoveState: request.LastMoveState
        );
        BitMove? bestMove = _aiEngine.FindBestMove(board, depth: Depth);
        if (bestMove is null)
        {
            return ValueTask.FromResult<AiEngineMoveReply?>(null);
        }

        List<AlgebraicPoint> captures = [];
        UInt128 captureMask = bestMove.Value.CapturesMask;
        while (captureMask != 0)
        {
            byte captureSquare = (byte)BitboardHelpers.BitScanForward(ref captureMask);
            captures.Add(AlgebraicPoint.FromIdx(captureSquare));
        }

        AiEngineMoveReply reply = new(
            From: AlgebraicPoint.FromIdx(bestMove.Value.From),
            To: AlgebraicPoint.FromIdx(bestMove.Value.To),
            Captures: captures,
            PromotesTo: bestMove.Value.PromotesTo
        );
        return ValueTask.FromResult<AiEngineMoveReply?>(reply);
    }
}
