using AnarchyChess.Api.Analysis.Models;
using AnarchyChess.Api.Analysis.Services;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.Analysis;

public class PositionAnalysisTests : BaseIntegrationTest
{
    private readonly IPlayableMoveProvider _playableMoveProvider;
    private readonly IPositionAnalysis _positionAnalysis;
    private readonly IFenEncoder _fenEncoder;
    private readonly IGameCore _core;

    public PositionAnalysisTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _playableMoveProvider = Scope.ServiceProvider.GetRequiredService<IPlayableMoveProvider>();
        _positionAnalysis = Scope.ServiceProvider.GetRequiredService<IPositionAnalysis>();
        _fenEncoder = Scope.ServiceProvider.GetRequiredService<IFenEncoder>();
        _core = Scope.ServiceProvider.GetRequiredService<IGameCore>();
    }

    [Fact]
    public void GetInitialPosition_returns_correct_starting_position()
    {
        var result = _positionAnalysis.GetInitialPosition();

        GameCoreState state = new();
        var fen = _core.StartGame(state);
        var legalMoves = _core.GetLegalMoves(state);
        MoveOptions moveOptions = new(legalMoves.MovePaths, legalMoves.HasForcedMoves);

        RootAnalysisPosition expectedPosition = new(Fen: fen.FullFen, MoveOptions: moveOptions);
        result.Should().BeEquivalentTo(expectedPosition);
    }

    [Fact]
    public void GetNextAnalysisPosition_returns_correct_position_after_move()
    {
        var initialPosition = _positionAnalysis.GetInitialPosition();
        MoveKey moveKey = new(from: new AlgebraicPoint("e2"), to: new AlgebraicPoint("e4"));
        AnalysisMove analysisMove = new(
            Fen: initialPosition.Fen,
            PiecePosition: new AlgebraicPoint("e2"),
            MoveKey: moveKey
        );

        var result = _positionAnalysis.GetNextAnalysisPosition(analysisMove);

        result.IsError.Should().BeFalse();
        var newPosition = result.Value;

        GameCoreState state = new();
        _core.StartGame(state);
        _core.MakeMove(moveKey, state);

        var legalMoves = _core.GetLegalMoves(state);
        MoveOptions moveOptions = new(legalMoves.MovePaths, legalMoves.HasForcedMoves);

        var fen = _fenEncoder.EncodeFen(state.Board);

        AnalysisPosition expectedPosition = new(
            Fen: fen.FullFen,
            San: "e4",
            MoveOptions: moveOptions,
            SideToMove: GameColor.Black,
            EndStatus: null
        );
        newPosition.Should().BeEquivalentTo(expectedPosition);
    }

    [Fact]
    public void GetNextAnalysisPosition_returns_error_for_invalid_fen()
    {
        var result = _positionAnalysis.GetNextAnalysisPosition(
            new(Fen: "invalid fen string", PiecePosition: new("a1"), MoveKey: "")
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenParts);
    }

    [Fact]
    public void GetNextAnalysisPosition_returns_error_for_invalid_move()
    {
        var result = _positionAnalysis.GetNextAnalysisPosition(
            new(Fen: GameTestData.InitialFen, PiecePosition: new("a1"), MoveKey: "invalid move key")
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public void GetNextLegalMoves_returns_move_options_for_valid_fen()
    {
        ChessBoard board = new();
        var fenResult = _fenEncoder.EncodeFen(board);
        var fen = fenResult.FullFen;

        var result = _positionAnalysis.GetNextLegalMoves(fen);

        result.IsError.Should().BeFalse();
        var moveOptions = result.Value;

        var expectedMoves = _playableMoveProvider.CalculateAllPlayableMoves(board);
        moveOptions.LegalMoves.Should().BeEquivalentTo(expectedMoves.MovePaths);
        moveOptions.HasForcedMoves.Should().Be(expectedMoves.HasForcedMoves);
    }

    [Fact]
    public void GetNextLegalMoves_returns_error_for_invalid_fen()
    {
        var result = _positionAnalysis.GetNextLegalMoves("invalid fen string");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenParts);
    }
}
