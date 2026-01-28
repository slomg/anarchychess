using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests.GameHandlersTests;

public class ClockHandlerTests : BaseIntegrationTest
{
    private readonly ClockHandler _handler;

    private readonly GameSettings _settings;
    private readonly IGameResultDescriber _resultDescriber;
    private readonly Overtime _overtime;
    private readonly GameClock _clock;
    private readonly IGameCore _core;

    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IGameNotifier _notifierMock = Substitute.For<IGameNotifier>();

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly GameToken _gameToken = "testtoken";

    private readonly GameData _gameData;

    public ClockHandlerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Game;
        _clock = new(settings, _timeProviderMock);
        _core = Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _resultDescriber = Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();
        _overtime = new(
            settings,
            Scope.ServiceProvider.GetRequiredService<IRandomProvider>(),
            _timeProviderMock,
            Scope.ServiceProvider.GetRequiredService<IPlayableMoveProvider>(),
            Scope.ServiceProvider.GetRequiredService<IMoveEncoder>()
        );

        _handler = new(_clock, _core, _overtime, _notifierMock, _resultDescriber);

        _gameData = GameUtils.CreateGameData(_core, _clock);
    }

    [Fact]
    public void GetClockDueTime_returns_time_left_from_real_clock_when_not_in_overtime()
    {
        var expectedMs = _clock.CalculateTimeLeftMs(GameColor.White, _gameData.ClockState);

        var result = _handler.GetClockDueTime(_gameData);

        result.TotalMilliseconds.Should().Be(expectedMs);
    }

    [Fact]
    public async Task OnClockTickAsync_reschedules_to_clock_timeout()
    {
        var gameData = GameUtils.CreateGameData(
            _core,
            _clock,
            timeControl: new(BaseSeconds: 10, IncrementSeconds: 0)
        );

        GetOutOfGracePeriod(gameData);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(5));

        var (rescheduleTo, endResult) = await _handler.OnClockTickAsync(_gameToken, gameData);

        endResult.Should().BeNull();
        rescheduleTo.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task OnClockTickAsync_aborts_after_timeout_in_grace_period_for_white()
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.Add(_settings.FirstMoveGracePeriod));

        var (rescheduleTo, endResult) = await _handler.OnClockTickAsync(_gameToken, _gameData);

        rescheduleTo.Should().BeNull();
        endResult.Should().Be(_resultDescriber.Aborted(by: GameColor.White));
    }

    [Fact]
    public async Task OnClockTickAsync_aborts_after_timeout_in_grace_period_for_black()
    {
        _clock.CommitTurn(GameColor.White, _gameData.ClockState);
        _core.MakeMove(GameUtils.GetLegalMove(_core, _gameData), _gameData.Core);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.Add(_settings.FirstMoveGracePeriod));

        var (rescheduleTo, endResult) = await _handler.OnClockTickAsync(_gameToken, _gameData);

        rescheduleTo.Should().BeNull();
        endResult.Should().Be(_resultDescriber.Aborted(by: GameColor.Black));
    }

    [Fact]
    public async Task OnClockTickAsync_does_not_abort_non_ticking_player_in_grace()
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.Add(_settings.FirstMoveGracePeriod));
        _core.MakeMove(GameUtils.GetLegalMove(_core, _gameData), _gameData.Core);
        _clock.CommitTurn(GameColor.White, _gameData.ClockState);

        var (rescheduleTo, endResult) = await _handler.OnClockTickAsync(_gameToken, _gameData);

        endResult.Should().BeNull();
        rescheduleTo.Should().NotBeNull();
    }

    [Fact]
    public async Task OnClockTickAsync_starts_overtime_when_necessary()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("b1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("h8"), PieceFactory.Black(PieceType.King));
        var gameData = GameUtils.CreateGameData(_core, _clock, board: board);

        GetOutOfGracePeriod(gameData);
        _timeProviderMock
            .GetUtcNow()
            .Returns(_fakeNow.AddSeconds(gameData.Pool.TimeControl.BaseSeconds));
        _overtime.HasEnteredOvertime(GameColor.White, gameData.OvertimeState).Should().BeFalse();

        var (rescheduleTo, endResult) = await _handler.OnClockTickAsync(_gameToken, gameData);

        rescheduleTo.Should().Be(_settings.OvertimeRemovalInterval);
        endResult.Should().BeNull();
        _overtime.HasEnteredOvertime(GameColor.White, gameData.OvertimeState).Should().BeTrue();
        await _notifierMock
            .Received(1)
            .NotifyNextOvertimeAsync(
                gameData.Players.WhitePlayer.UserId,
                plyNumber: gameData.MoveHistory.Moves.Count,
                removeFrom: Arg.Is<AlgebraicPoint>(p =>
                    p == new AlgebraicPoint("a1") || p == new AlgebraicPoint("b1")
                ),
                gameToken: _gameToken
            );
    }

    [Fact]
    public async Task OnClockTickAsync_tracks_overtime_removals()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("a4"), PieceFactory.Black(PieceType.King));
        var gameData = GameUtils.CreateGameData(_core, _clock, board: board);

        GetOutOfGracePeriod(gameData);
        _timeProviderMock
            .GetUtcNow()
            .Returns(_fakeNow.AddSeconds(gameData.Pool.TimeControl.BaseSeconds));

        // add move to make sure overtime removals are added
        gameData.MoveHistory.AddMove(
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

        var (rescheduleTo, endStatus) = await _handler.OnClockTickAsync(_gameToken, gameData);

        rescheduleTo.Should().Be(_settings.OvertimeRemovalInterval);
        endStatus.Should().BeNull();

        await _notifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyOvertimeAsync(default, default, default!, default, default!);
        gameData.MoveHistory.Moves[^1].Path.OvertimeRemovalIdxs.Should().BeEmpty();
        var prevLegalMoves = _core.GetLegalMoves(gameData.Core);

        await _handler.OnClockTickAsync(_gameToken, gameData);

        await _notifierMock
            .Received(1)
            .NotifyOvertimeAsync(
                plyNumber: gameData.MoveHistory.Moves.Count,
                removeFrom: new("a1"),
                Arg.Any<CompressedMoves>(),
                _gameToken,
                gameData.NotifierState
            );
        gameData.MoveHistory.Moves[^1].Path.OvertimeRemovalIdxs.Should().HaveCount(1);
        var newBoard = _core.GetReadOnlyBoard(gameData.Core);
        newBoard.IsEmpty(new("a1")).Should().BeTrue();
        _core.GetLegalMoves(gameData.Core).Should().NotBeEquivalentTo(prevLegalMoves);
    }

    [Fact]
    public async Task OnClockTickAsync_ends_game_for_overtime_when_needed()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("a3"), PieceFactory.Black(PieceType.King));
        var gameData = GameUtils.CreateGameData(_core, _clock, board: board);

        GetOutOfGracePeriod(gameData);
        _timeProviderMock
            .GetUtcNow()
            .Returns(_fakeNow.AddSeconds(gameData.Pool.TimeControl.BaseSeconds));

        await _handler.OnClockTickAsync(_gameToken, gameData);

        var (overtimeRescheduleTo, overtimeEndStatus) = await _handler.OnClockTickAsync(
            _gameToken,
            gameData
        );

        overtimeRescheduleTo.Should().BeNull();
        overtimeEndStatus.Should().Be(_resultDescriber.Overtime(by: GameColor.White));
    }

    [Fact]
    public async Task GetClockDueTime_uses_normal_clock_when_other_player_is_in_overtime()
    {
        GetOutOfGracePeriod(_gameData);

        // white timeout
        _timeProviderMock
            .GetUtcNow()
            .Returns(_fakeNow.AddSeconds(_gameData.Pool.TimeControl.BaseSeconds));
        await _handler.OnClockTickAsync(_gameToken, _gameData);

        _core.MakeMove(GameUtils.GetLegalMove(_core, _gameData), _gameData.Core);
        _clock.CommitTurn(GameColor.White, _gameData.ClockState);

        var result = _handler.GetClockDueTime(_gameData);

        var expectedMs = _clock.CalculateTimeLeftMs(GameColor.Black, _gameData.ClockState);
        expectedMs.Should().BeGreaterThan(10);
        result.TotalMilliseconds.Should().Be(expectedMs);
    }

    private void GetOutOfGracePeriod(GameData gameData)
    {
        _core.MakeMove(GameUtils.GetLegalMove(_core, _gameData), _gameData.Core);
        _clock.CommitTurn(GameColor.White, gameData.ClockState);
        _core.MakeMove(GameUtils.GetLegalMove(_core, _gameData), _gameData.Core);
        _clock.CommitTurn(GameColor.Black, gameData.ClockState);
    }
}
