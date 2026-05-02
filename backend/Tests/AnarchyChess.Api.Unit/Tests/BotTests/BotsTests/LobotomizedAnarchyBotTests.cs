using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.BotTests.BotsTests;

public class LobotomizedAnarchyBotTests
{
    private readonly LobotomizedAnarchyBot _bot;

    public LobotomizedAnarchyBotTests()
    {
        _bot = new(Substitute.For<IBotDecisionServiceFactory>());
    }

    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    public void CreateBotPlayer_creates_player(GameColor color)
    {
        GamePlayer result = _bot.CreateBotPlayer(color);

        GamePlayer expectedResult = new(
            UserId: "bot:lobotomized-anarchybot",
            Color: color,
            UserName: "Lobotomized Anarchy Bot",
            CountryCode: "FR",
            Rating: -161660
        );
        result.Should().BeEquivalentTo(expectedResult);
    }
}
