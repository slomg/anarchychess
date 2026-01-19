using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.SanNotation;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using ErrorOr;

namespace AnarchyChess.Api.Game.Services;

public interface IGameCore
{
    LegalMoveSet GetLegalMoves(GameCoreState state);
    ErrorOr<MoveResult> MakeMove(MoveKey key, GameCoreState state);
    MoveResult MakeMove(Move move, GameCoreState state);
    GameColor SideToMove(GameCoreState state);
    FenNotation StartGame(GameCoreState state);
}

public readonly record struct MoveResult(
    Move Move,
    MovePath MovePath,
    FenNotation Fen,
    string San,
    GameEndStatus? EndStatus
);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.GameCoreState")]
public class GameCoreState
{
    [Id(0)]
    public ChessBoard Board { get; init; } =
        new(
            GameConstants.StartingPosition,
            GameLogicConstants.BoardHeight,
            GameLogicConstants.BoardWidth
        );

    [Id(1)]
    public LegalMoveSet LegalMoves { get; set; } = new();

    [Id(2)]
    public AutoDrawState AutoDrawState { get; set; } = new();
}

public class GameCore(
    ILogger<GameCore> logger,
    IFenEncoder fenEncoder,
    IPlayableMoveProvider playableMoveProvider,
    ISanCalculator sanCalculator,
    IDrawEvaulator drawEvaulator,
    IGameResultDescriber resultDescriber
) : IGameCore
{
    private readonly ILogger<GameCore> _logger = logger;
    private readonly IFenEncoder _fenEncoder = fenEncoder;
    private readonly IPlayableMoveProvider _playableMoveProvider = playableMoveProvider;
    private readonly ISanCalculator _sanCalculator = sanCalculator;
    private readonly IDrawEvaulator _drawEvaulator = drawEvaulator;
    private readonly IGameResultDescriber _resultDescriber = resultDescriber;

    public GameColor SideToMove(GameCoreState state) => state.Board.SideToMove;

    public FenNotation StartGame(GameCoreState state)
    {
        var fen = _fenEncoder.EncodeFen(state.Board);
        state.LegalMoves = _playableMoveProvider.CalculateAllPlayableMoves(state.Board);
        _drawEvaulator.RegisterInitialPosition(fen, state.AutoDrawState);

        return fen;
    }

    public ErrorOr<MoveResult> MakeMove(MoveKey key, GameCoreState state)
    {
        if (!state.LegalMoves.MoveMap.TryGetValue(key, out var move))
        {
            _logger.LogWarning("Could not find move with key {Key}", key);
            return GameErrors.MoveInvalid;
        }

        return MakeMove(move, state);
    }

    public MoveResult MakeMove(Move move, GameCoreState state)
    {
        var movingSide = state.Board.SideToMove;
        state.Board.PlayMove(move);
        var fen = _fenEncoder.EncodeFen(state.Board);

        GameEndStatus? endStatus = null;
        var kingCaptureWinStatus = EvaluateKingCaptureResult(move, state.Board, movingSide);
        if (kingCaptureWinStatus is not null)
        {
            endStatus = kingCaptureWinStatus;
        }
        else if (
            _drawEvaulator.TryEvaluateDraw(
                move,
                fen,
                state.Board,
                state.AutoDrawState,
                out var drawReason
            )
        )
        {
            endStatus = drawReason;
        }

        var path = MovePath.FromMove(move, state.Board.Width);
        var san = _sanCalculator.CalculateSan(
            move,
            state.LegalMoves.AllMoves,
            isKingCapture: kingCaptureWinStatus is not null
                && kingCaptureWinStatus.Result is not GameResult.Draw,
            isDraw: endStatus?.Result is GameResult.Draw
        );
        MoveResult moveResult = new(
            Move: move,
            MovePath: path,
            Fen: fen,
            San: san,
            EndStatus: endStatus
        );

        state.LegalMoves = _playableMoveProvider.CalculateAllPlayableMoves(state.Board);
        return moveResult;
    }

    public LegalMoveSet GetLegalMoves(GameCoreState state) => state.LegalMoves;

    private GameEndStatus? EvaluateKingCaptureResult(
        Move move,
        ChessBoard board,
        GameColor movingSide
    )
    {
        if (
            move.Captures is null
            || !move.Captures.Any(x => x.CapturedPiece.Type is PieceType.King)
        )
            return null;

        bool isOpponentKingCapture = !board.HasPieceWith(PieceType.King, movingSide.Invert());
        bool isSelfCapture = !board.HasPieceWith(PieceType.King, movingSide);
        if (isOpponentKingCapture && isSelfCapture)
            return _resultDescriber.MutualKingCapture();

        if (isOpponentKingCapture)
            return _resultDescriber.KingCaptured(by: movingSide);

        if (isSelfCapture)
            return _resultDescriber.KingSelfCapture(by: movingSide);

        return null;
    }
}
