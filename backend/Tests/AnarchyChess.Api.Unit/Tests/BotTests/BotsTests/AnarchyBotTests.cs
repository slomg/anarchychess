using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.BotTests.BotsTests;

public class AnarchyBotTests
{
    private readonly IBotService _botServiceMock = Substitute.For<IBotService>();

    private readonly AnarchyBot _bot;

    public AnarchyBotTests()
    {
        _bot = new(_botServiceMock);
    }

    [Fact]
    public async Task FindMoveAsync_returns_best_move()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>() { [new("b5")] = PieceFactory.White() }
        );
        MoveEvaluation moveEval = new MoveEvaluationFaker().Generate();
        _botServiceMock
            .FindBestMoveAsync(board, depth: 8, TestContext.Current.CancellationToken)
            .Returns(moveEval);

        var result = await _bot.FindMoveAsync(
            board,
            lastEval: 0,
            TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(moveEval);
    }

    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    public void CreateBotPlayer_creates_player(GameColor color)
    {
        GamePlayer result = _bot.CreateBotPlayer(color);

        GamePlayer expectedResult = new(
            UserId: "bot:anarchybot",
            Color: color,
            UserName: "Anarchy Bot",
            CountryCode: "XX",
            Rating: 161660
        );
        result.Should().BeEquivalentTo(expectedResult);
    }
}
