using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

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
            prevMoveState: CreatePrevMove(request.PrevMoveState)
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

    private static PrevMoveState? CreatePrevMove(PrevMoveStateDto? prevMoveState)
    {
        if (prevMoveState is null)
        {
            return null;
        }

        UInt128 captureMask = 0;
        foreach (AlgebraicPoint capture in prevMoveState.LastCaptures)
        {
            captureMask |= UInt128.One << capture.AsIdx();
        }

        BitPieceColor color = prevMoveState.Piece.Color.Match(
            whenWhite: BitPieceColor.White,
            whenBlack: BitPieceColor.Black,
            whenNeutral: BitPieceColor.Neutral
        );
        BitPiece piece = new() { Type = prevMoveState.Piece.Type, Color = color };
        return new(
            From: prevMoveState.From.AsIdx(),
            To: prevMoveState.To.AsIdx(),
            Piece: piece,
            CaptureMask: captureMask
        );
    }
}
