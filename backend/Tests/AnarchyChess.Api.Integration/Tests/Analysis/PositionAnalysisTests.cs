using AnarchyChess.Api.Analysis.Models;
using AnarchyChess.Api.Analysis.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.Analysis;

public class PositionAnalysisTests : BaseIntegrationTest
{
    private readonly IPositionAnalysis _positionAnalysis;
    private readonly IFenEncoder _fenEncoder;
    private readonly IGameCore _core;

    public PositionAnalysisTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
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
        var legalMoves = _core.GetLegalMovesOf(GameColor.White, state);
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

        var legalMoves = _core.GetLegalMovesOf(GameColor.Black, state);
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
    public void GetNextAnalysisPosition_doesnt_include_legal_moves_when_the_game_ends()
    {
        AnalysisMove analysisMove = new(
            "qK {\"sideToMove\":1}", // black queen next to white king
            new AlgebraicPoint("a1"),
            new MoveKey(new AlgebraicPoint("a1"), new AlgebraicPoint("b1"))
        );

        var result = _positionAnalysis.GetNextAnalysisPosition(analysisMove);

        result.IsError.Should().BeFalse();

        var newPosition = result.Value;
        newPosition.MoveOptions.Should().BeEquivalentTo(new MoveOptions());
        newPosition.EndStatus.Should().NotBeNull();
        newPosition.EndStatus.Result.Should().Be(GameResult.BlackWin);
        newPosition.SideToMove.Should().Be(GameColor.White);
    }
}
