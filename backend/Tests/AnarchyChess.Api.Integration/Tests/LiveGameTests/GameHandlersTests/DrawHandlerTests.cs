using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests.GameHandlersTests;

public class DrawHandlerTests : BaseIntegrationTest
{
    private readonly IGameResultDescriber _gameResultDescriber;

    private readonly GameSettings _settings;
    private readonly DrawHandler _handler;

    private readonly GameToken _gameToken = "testtoken";

    private readonly IGameNotifier _notifierMock = Substitute.For<IGameNotifier>();

    private readonly GameData _gameData;

    public DrawHandlerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameResultDescriber = Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Game;

        _handler = new(settings, _gameResultDescriber, _notifierMock);

        _gameData = GameUtils.CreateGameData(
            Scope.ServiceProvider.GetRequiredService<IGameCore>(),
            Scope.ServiceProvider.GetRequiredService<IGameClock>()
        );
    }

    [Fact]
    public async Task HandleDrawRequestAsync_sends_notification_if_no_pending_request()
    {
        var result = await _handler.HandleDrawRequestAsync(
            _gameData.Players.WhitePlayer,
            _gameToken,
            _gameData
        );
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();

        await _notifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                _gameToken,
                new DrawState(ActiveRequester: GameColor.White),
                _gameData.NotifierState
            );

        _gameData.DrawRequest.ActiveRequester.Should().Be(GameColor.White);
    }

    [Fact]
    public async Task RequestDrawAsync_ends_the_game_if_there_is_a_pending_request()
    {
        var requestResult = await _handler.HandleDrawRequestAsync(
            _gameData.Players.WhitePlayer,
            _gameToken,
            _gameData
        );
        var acceptResult = await _handler.HandleDrawRequestAsync(
            _gameData.Players.BlackPlayer,
            _gameToken,
            _gameData
        );

        requestResult.IsError.Should().BeFalse();
        requestResult.Value.Should().BeNull();

        acceptResult.IsError.Should().BeFalse();
        acceptResult.Value.Should().Be(_gameResultDescriber.DrawByAgreement());
    }

    [Fact]
    public async Task DeclineDrawAsync_declines_the_draw_correctly()
    {
        await _handler.HandleDrawRequestAsync(_gameData.Players.WhitePlayer, _gameToken, _gameData);

        var result = await _handler.HandleDeclineDrawAsync(
            _gameData.Players.BlackPlayer,
            _gameToken,
            _gameData
        );

        result.IsError.Should().BeFalse();

        await _notifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                _gameToken,
                new DrawState(WhiteCooldown: _settings.DrawCooldown),
                _gameData.NotifierState
            );
        _gameData.DrawRequest.ActiveRequester.Should().BeNull();
    }
}
