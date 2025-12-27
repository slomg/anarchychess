using AnarchyChess.Api.Analysis.Models;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using ErrorOr;

namespace AnarchyChess.Api.Analysis.Services;

public interface IPositionAnalysis
{
    AnalysisPosition GetInitialPosition();
    ErrorOr<AnalysisPosition> GetNextLegalMoves(AnalysisMove analMove);
}

public class PositionAnalysis(
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IGameCore gameCore
) : IPositionAnalysis
{
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IGameCore _core = gameCore;

    public AnalysisPosition GetInitialPosition()
    {
        GameCoreState coreState = new();
        var initialFen = _core.StartGame(coreState);

        return GetAnalysisPosition(coreState, fen: initialFen);
    }

    public ErrorOr<AnalysisPosition> GetNextLegalMoves(AnalysisMove analMove) // hehe
    {
        var boardResult = _fenCalculator.DecodeFen(analMove.Fen, sideToMove: analMove.MovingPlayer);
        if (boardResult.IsError)
            return boardResult.Errors;
        var board = boardResult.Value;

        var move = _legalMoveCalculator
            .CalculateLegalMoves(board, analMove.PiecePosition, analMove.MovingPlayer)
            .FirstOrDefault(x => new MoveKey(x) == analMove.MoveKey);
        if (move is null)
            return GameErrors.MoveInvalid;

        GameCoreState coreState = new() { Board = board };
        var moveResult = _core.MakeMove(move, coreState);
        var newFen = _fenCalculator.CalculateFen(coreState.Board);

        return GetAnalysisPosition(coreState, newFen, moveResult);
    }

    private AnalysisPosition GetAnalysisPosition(
        GameCoreState state,
        string fen,
        MoveResult? lastMove = null
    )
    {
        var sideToMove = _core.SideToMove(state);
        var legalMoves = _core.GetLegalMovesOf(sideToMove, state);
        MoveOptions moveOptions = new(
            LegalMoves: legalMoves.MovePaths,
            HasForcedMoves: legalMoves.HasForcedMoves
        );

        return new(
            Fen: fen,
            San: lastMove?.San,
            MoveOptions: moveOptions,
            SideToMove: sideToMove,
            EndStatus: lastMove?.EndStatus
        );
    }
}
