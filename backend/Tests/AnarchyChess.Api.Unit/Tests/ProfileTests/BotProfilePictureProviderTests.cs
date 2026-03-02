using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Profile.Services;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.ProfileTests;

public class BotProfilePictureProviderTests
{
    private readonly BotProfilePictureProvider _provider = new();

    [Fact]
    public void GetBotProfilePicture_returns_bytes_for_bot()
    {
        var result = _provider.GetBotProfilePicture(UserId.AnarchyBot());

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetBotProfilePicture_returns_a_different_profile_picture_for_each_bot()
    {
        var result1 = _provider.GetBotProfilePicture(UserId.AnarchyBot());
        var result2 = _provider.GetBotProfilePicture(UserId.LobotomizedAnarchyBot());

        result1.Should().NotBeNullOrEmpty();
        result2.Should().NotBeNullOrEmpty();
        result1.Should().NotBeEquivalentTo(result2);
    }

    [Fact]
    public void GetBotProfilePicture_returns_null_for_non_bots()
    {
        var result = _provider.GetBotProfilePicture(UserId.Authed());

        result.Should().BeNull();
    }

    [Fact]
    public void GetBotProfilePicture_returns_none_for_unknown_bot()
    {
        var result = _provider.GetBotProfilePicture("bot:test");

        result.Should().BeNull();
    }
}
