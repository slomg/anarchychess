using AnarchyChess.Api.Analysis.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests.Analysis;

public class AnalysisControllerTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetInitialAnalysisPosition_returns_initial_position()
    {
        var response = await ApiClient.Api.GetInitialAnalysisPosition();

        response.IsSuccessful.Should().BeTrue();

        var position = response.Content;
        position.Should().NotBeNull();
        position.LegalMoves.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetNextAnalysisPosition_plays_the_move()
    {
        AnalysisMove move = new(
            Fen: "R5rKk",
            PiecePosition: new AlgebraicPoint("a1"),
            MoveKey: new(from: new AlgebraicPoint("a1"), to: new AlgebraicPoint("c1"))
        );

        var response = await ApiClient.Api.GetNextAnalysisPosition(move);

        response.IsSuccessful.Should().BeTrue();

        var position = response.Content;
        position.Should().NotBeNull();
        position.San.Should().Be("Rc1");
        position.SideToMove.Should().Be(GameColor.Black);
        position.LegalMoves.Count.Should().BeGreaterThan(0);
        position.EndStatus.Should().BeNull();
    }

    [Fact]
    public async Task GetNextLegalMoves_returns_legal_moves()
    {
        var response = await ApiClient.Api.GetNextLegalMoves(GameTestData.InitialFen);

        response.IsSuccessful.Should().BeTrue();

        var legalMoves = response.Content;
        legalMoves.Should().NotBeNull();
        legalMoves.Count.Should().BeGreaterThan(0);
    }
}
