using AnarchyChess.Api.AnarchyBot.Grains;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Functional.Tests.AnarchyBotTests;

public class AnarchyBotControllerTests : BaseFunctionalTest
{
    private readonly IGrainFactory _grains;

    public AnarchyBotControllerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _grains = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
    }

    [Fact]
    public async Task StartBotGame_starts_bot_game_for_guest()
    {
        var guest = UserId.Guest();
        AuthUtils.AuthenticateGuest(ApiClient, guest);

        var response = await ApiClient.Api.StartAnarchyBotGameAsync(myColor: GameColor.White);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        string gameToken = response.Content;

        var grain = _grains.GetGrain<IAnarchyBotGrain>(gameToken);
        var stateResult = await grain.GetStateAsync(CT);
        stateResult.IsError.Should().BeFalse();
        var state = stateResult.Value;

        state.WhitePlayer.UserId.Should().Be(guest);
        state.WhitePlayer.UserName.Should().Be("Guest");
        state.WhitePlayer.CountryCode.Should().Be("XX");

        state.BlackPlayer.UserId.Should().Be(UserId.AnarchyBot());
    }

    [Fact]
    public async Task StartBotGame_starts_bot_game_for_authed()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var response = await ApiClient.Api.StartAnarchyBotGameAsync(myColor: GameColor.White);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        string gameToken = response.Content;

        var grain = _grains.GetGrain<IAnarchyBotGrain>(gameToken);
        var stateResult = await grain.GetStateAsync(CT);
        stateResult.IsError.Should().BeFalse();
        var state = stateResult.Value;

        state.WhitePlayer.UserId.Should().Be(user.Id);
        state.WhitePlayer.UserName.Should().Be(user.UserName);
        state.WhitePlayer.CountryCode.Should().Be(user.CountryCode);

        state.BlackPlayer.UserId.Should().Be(UserId.AnarchyBot());
    }
}
