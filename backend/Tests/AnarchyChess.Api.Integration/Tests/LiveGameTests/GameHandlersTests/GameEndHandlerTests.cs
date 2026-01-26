using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests.GameHandlersTests;

public class GameEndHandlerTests : BaseIntegrationTest
{
    private readonly GameEndHandler _handler;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();
    private readonly IGameFinalizer _gameFinalizerMock = Substitute.For<IGameFinalizer>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly GameSettings _settings;
    private readonly IGameCore _core;
    private readonly IGameClock _clock;
    private readonly Overtime _overtime;

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly GameState _gameState = new GameStateFaker().Generate();
    private readonly GameData _gameData;
    private readonly GameToken _gameToken = "testtoken";
    private readonly GameEndStatus _endStatus = new(GameResult.WhiteWin, "desc");

    public GameEndHandlerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Game;

        _overtime = new(
            settings,
            Scope.ServiceProvider.GetRequiredService<IRandomProvider>(),
            _timeProviderMock,
            Scope.ServiceProvider.GetRequiredService<IPlayableMoveProvider>(),
            Scope.ServiceProvider.GetRequiredService<IMoveEncoder>()
        );

        _core = Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _clock = Scope.ServiceProvider.GetRequiredService<IGameClock>();

        _handler = new(_core, _clock, _overtime, _gameNotifierMock, _gameFinalizerMock);

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

    [Fact]
    public async Task HandleGameEndAsync_removes_overtime_pieces()
    {
        // add move to make sure overtime removals are added
        _gameData.MoveHistory.AddMove(
            GameColor.Black,
            new MoveResult(
                new MoveFaker().Generate(),
                new MovePathFaker().RuleFor(x => x.OvertimeRemovalIdxs, []).Generate(),
                new FenNotation("a", "b"),
                "e4",
                EndStatus: null
            ),
            timeLeft: 123
        );
        var now = _fakeNow.AddSeconds(_gameData.Pool.TimeControl.BaseSeconds);
        _timeProviderMock.GetUtcNow().Returns(now);

        _overtime.StartOvertimeTurn(GameColor.White, _gameData.Core.Board, _gameData.OvertimeState);
        now += _overtime.GetTimeUntilDefeat(GameColor.White, _gameData.OvertimeState)!.Value;
        _timeProviderMock.GetUtcNow().Returns(now);

        var (pendingRemoval, newLegalMoves) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _gameData.OvertimeState
        );

        await _handler.HandleGameEndAsync(_gameState, _endStatus, _gameToken, _gameData, CT);

        foreach (var point in pendingRemoval)
        {
            _gameData.Core.Board.IsEmpty(point).Should().BeTrue();
        }
        _core.GetLegalMoves(_gameData.Core).Should().BeEquivalentTo(newLegalMoves);
        _gameData
            .MoveHistory.Moves[^1]
            .Path.OvertimeRemovalIdxs.Should()
            .HaveCount(pendingRemoval.Count);
        _gameData.OvertimeState.PlayerOvertime.Should().BeEmpty();
    }
}
