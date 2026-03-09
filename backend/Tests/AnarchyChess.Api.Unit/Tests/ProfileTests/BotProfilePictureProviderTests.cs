using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Profile.Services;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.ProfileTests;

public class BotProfilePictureProviderTests
{
    private readonly BotProfilePictureProvider _provider = new();

    private static readonly string _baseDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Bots"
    );

    [Theory]
    [MemberData(nameof(BotData))]
    public void GetBotProfilePicture_returns_bytes_for_bot(UserId botId, string path)
    {
        var result = _provider.GetBotProfilePictureBytes(botId);

        byte[] expectedResult = File.ReadAllBytes(path);
        result.Should().Equal(expectedResult);
    }

    [Fact]
    public void GetBotProfilePicture_returns_none_for_unknown_bot()
    {
        var result = _provider.GetBotProfilePictureBytes("bot:test");

        result.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(BotData))]
    public void GetBotProfilePictureLastModified_returns_expected_value(UserId botId, string path)
    {
        var result = _provider.GetBotProfilePictureLastModified(botId);

        DateTimeOffset expectedResult = File.GetLastWriteTimeUtc(path);
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetBotProfilePictureLastModified_returns_min_value_for_unknown_bot()
    {
        var result = _provider.GetBotProfilePictureLastModified("bot:test");

        result.Should().Be(DateTimeOffset.MinValue);
    }

    public static IEnumerable<TheoryDataRow<UserId>> BotUserIdsData =>
        [new(UserId.AnarchyBot()), new(UserId.LobotomizedAnarchyBot())];

    public static IEnumerable<TheoryDataRow<UserId, string>> BotData =>
        [
            new(UserId.AnarchyBot(), Path.Combine(_baseDirectory, "anarchybot.webp")),
            new(
                UserId.LobotomizedAnarchyBot(),
                Path.Combine(_baseDirectory, "lobotomized-anarchybot.webp")
            ),
        ];
}
