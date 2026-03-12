using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;
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
            prevMoveState: CreatePrevMove(request.PrevMoveState)
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
            prevMoveState: CreatePrevMove(request.PrevMoveState)
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

    private static PrevMoveState? CreatePrevMove(PrevMoveStateDto? prevMoveState)
    {
        if (prevMoveState is null)
        {
            return null;
        }

        UInt128 captureMask = 0;
        foreach (AlgebraicPoint capture in prevMoveState.Captures ?? [])
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
