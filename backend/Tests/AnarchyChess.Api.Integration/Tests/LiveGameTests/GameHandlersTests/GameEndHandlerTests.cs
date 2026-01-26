using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests.GameHandlersTests;

public class GameEndHandlerTests : BaseIntegrationTest
{
    private readonly GameEndHandler _handler;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();
    private readonly IGameFinalizer _gameFinalizerMock = Substitute.For<IGameFinalizer>();

    private readonly IGameCore _core;
    private readonly IGameClock _clock;

    private readonly GameState _gameState = new GameStateFaker().Generate();
    private readonly GameData _gameData;
    private readonly GameToken _gameToken = "testtoken";
    private readonly GameEndStatus _endStatus = new(GameResult.WhiteWin, "desc");

    public GameEndHandlerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _core = Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _clock = Scope.ServiceProvider.GetRequiredService<IGameClock>();

        _handler = new(_core, _clock, _gameNotifierMock, _gameFinalizerMock);

        _gameData = GameUtils.CreateGameData(_core, _clock);
    }

    [Fact]
    public async Task HandleGamEndAsync_commits_last_turn()
    {
        await _handler.HandleGameEndAsync(_gameState, _endStatus, _gameToken, _gameData, CT);

        _gameData.ClockState.IsFrozen.Should().BeTrue();
    }

    [Fact]
    public async Task HandleGameEndAsync_finalizes_the_game()
    {
        var resultData = new GameResultDataFaker().Generate();
        _gameFinalizerMock
            .FinalizeGameAsync(_gameToken, _gameState, _endStatus, CT)
            .Returns(resultData);

        var result = await _handler.HandleGameEndAsync(
            _gameState,
            _endStatus,
            _gameToken,
            _gameData,
            CT
        );

        result.Should().Be(resultData);
    }

    [Fact]
    public async Task HandleGameEndAsync_sends_notification()
    {
        var resultData = new GameResultDataFaker().Generate();
        _gameFinalizerMock
            .FinalizeGameAsync(_gameToken, _gameState, _endStatus, CT)
            .Returns(resultData);

        await _handler.HandleGameEndAsync(_gameState, _endStatus, _gameToken, _gameData, CT);

        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                _gameToken,
                resultData,
                _clock.ToSnapshot(_gameData.ClockState),
                _gameData.NotifierState
            );
    }
}
