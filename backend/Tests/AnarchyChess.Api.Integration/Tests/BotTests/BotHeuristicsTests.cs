using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.BotTests;

public class BotHeuristicsTests : BaseIntegrationTest
{
    private readonly IBotHeuristics _botHeuristics;

    public BotHeuristicsTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _botHeuristics = Scope.ServiceProvider.GetRequiredService<IBotHeuristics>();
    }

    [Fact]
    public void IsNonCentralPawn_returns_false_for_non_pawn()
    {
        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            from: new("a2"),
            to: new("b3")
        ).Generate();

        _botHeuristics.IsNonCentralPawn(move).Should().BeFalse();
    }

    [Theory]
    [InlineData("a2", "a4", true)]
    [InlineData("c2", "c5", true)]
    [InlineData("d8", "d6", false)]
    [InlineData("e3", "e4", false)]
    [InlineData("f2", "f4", false)]
    [InlineData("g7", "g6", false)]
    [InlineData("h2", "h5", true)]
    [InlineData("j8", "j6", true)]
    public void IsNonCentralPawn_detects_non_central_pawn_push(
        string from,
        string to,
        bool isNonCentral
    )
    {
        BitMove move = new BitMoveFaker(PieceType.Pawn, from: new(from), to: new(to)).Generate();

        _botHeuristics.IsNonCentralPawn(move).Should().Be(isNonCentral);
    }

    [Theory]
    [InlineData("b2", "a3", true)]
    [InlineData("i2", "j3", true)]
    [InlineData("a2", "b3", false)]
    [InlineData("j2", "i3", false)]
    [InlineData("e2", "e3", false)]
    public void IsEdge_detects_edge_moves(string from, string to, bool expected)
    {
        BitMove move = new BitMoveFaker(from: new(from), to: new(to)).Generate();
        _botHeuristics.IsEdge(move).Should().Be(expected);
    }
}
