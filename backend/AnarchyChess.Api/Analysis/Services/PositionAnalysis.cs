using AnarchyChess.Api.Analysis.Models;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using ErrorOr;

namespace AnarchyChess.Api.Analysis.Services;

public interface IPositionAnalysis
{
    RootAnalysisPosition GetInitialPosition();
    ErrorOr<AnalysisPosition> GetNextAnalysisPosition(AnalysisMove analMove);
    ErrorOr<MoveOptions> GetNextLegalMoves(string fen);
}

public class PositionAnalysis(
    IFenDecoder fenDecoder,
    IPlayableMoveProvider playableMoveProvider,
    IGameCore gameCore
) : IPositionAnalysis
{
    private readonly IFenDecoder _fenDecoder = fenDecoder;
    private readonly IPlayableMoveProvider _playableMoveProvider = playableMoveProvider;
    private readonly IGameCore _core = gameCore;

    public RootAnalysisPosition GetInitialPosition()
    {
        GameCoreState coreState = new();
        var initialFen = _core.StartGame(coreState);
        var legalMoves = _core.GetLegalMoves(coreState);
        MoveOptions moveOptions = new(
            LegalMoves: legalMoves.MovePaths,
            HasForcedMoves: legalMoves.HasForcedMoves
        );

        return new(Fen: initialFen.FullFen, MoveOptions: moveOptions);
    }

    public ErrorOr<AnalysisPosition> GetNextAnalysisPosition(AnalysisMove analMove) // hehe
    {
        var boardResult = _fenDecoder.DecodeFen(analMove.Fen);
        if (boardResult.IsError)
            return boardResult.Errors;
        var board = boardResult.Value;

        var move = _playableMoveProvider.GetPieceMoveByKey(
            board,
            analMove.PiecePosition,
            analMove.MoveKey
        );
        if (move is null)
            return GameErrors.MoveInvalid;

        GameCoreState coreState = new() { Board = board };
        var moveResult = _core.MakeMove(move, coreState);
        var sideToMove = _core.SideToMove(coreState);
        var legalMoves = _core.GetLegalMoves(coreState);
        MoveOptions moveOptions = new(
            LegalMoves: legalMoves.MovePaths,
            HasForcedMoves: legalMoves.HasForcedMoves
        );

        return new AnalysisPosition(
            Fen: moveResult.Fen.FullFen,
            San: moveResult.San,
            MoveOptions: moveOptions,
            SideToMove: sideToMove,
            EndStatus: moveResult.EndStatus
        );
    }

    public ErrorOr<MoveOptions> GetNextLegalMoves(string fen)
    {
        var boardResult = _fenDecoder.DecodeFen(fen);
        if (boardResult.IsError)
            return boardResult.Errors;
        var board = boardResult.Value;

        var legalMoves = _playableMoveProvider.CalculateAllPlayableMoves(board);
        MoveOptions moveOptions = new(
            LegalMoves: legalMoves.MovePaths,
            HasForcedMoves: legalMoves.HasForcedMoves
        );

        return moveOptions;
    }
}
