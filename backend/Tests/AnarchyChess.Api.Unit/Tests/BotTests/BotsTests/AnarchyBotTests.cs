using AnarchyChess.Ai.Service.DTO;
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
        AiEngineMove move = new AiEngineMoveFaker().Generate();
        _botServiceMock
            .FindBestMoveAsync(board, TestContext.Current.CancellationToken)
            .Returns(move);

        var result = await _bot.FindMoveAsync(board, TestContext.Current.CancellationToken);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(move);
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
