using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Streaming;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;
using Orleans.TestKit.Streams;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class GameGrainTests : BaseOrleansIntegrationTest
{
    private readonly GameToken _gameToken = "testtoken";
    private readonly PoolKey _pool = new(
        PoolType.Rated,
        new(BaseSeconds: 600, IncrementSeconds: 5)
    );

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly GameClock _gameClock;
    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly IGameCore _gameCore;
    private readonly Overtime _overtime;
    private readonly GameSettings _settings;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    private readonly GameGrainState _state;
    private readonly TestStorageStats _stateStats;

    private readonly TestStream<GameEndedEvent> _whiteGameEndedStream;
    private readonly TestStream<GameEndedEvent> _blackGameEndedStream;

    public GameGrainTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();

        var settings = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IOptions<AppSettings>
        >();
        var gameFinalizer = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameFinalizer>();
        _gameClock = new(settings, _timeProviderMock);

        _overtime = new(
            settings,
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IRandomProvider>(),
            _timeProviderMock,
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IPlayableMoveProvider>(),
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IMoveEncoder>()
        );
        MoveHandler moveHandler = new(
            Substitute.For<ILogger<MoveHandler>>(),
            settings,
            _gameCore,
            _gameClock,
            _gameNotifierMock,
            _overtime
        );
        DrawHandler drawHandler = new(settings, _gameResultDescriber, _gameNotifierMock);
        ClockHandler clockHandler = new(
            _gameClock,
            _gameCore,
            _overtime,
            _gameNotifierMock,
            _gameResultDescriber
        );

        _settings = settings.Value.Game;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_gameCore);
        Silo.ServiceProvider.AddService<IGameClock>(_gameClock);
        Silo.ServiceProvider.AddService(_gameResultDescriber);
        Silo.ServiceProvider.AddService(_gameNotifierMock);
        Silo.ServiceProvider.AddService(gameFinalizer);
        Silo.ServiceProvider.AddService(settings);
        Silo.ServiceProvider.AddService<IOvertime>(_overtime);
        Silo.ServiceProvider.AddService<IMoveHandler>(moveHandler);
        Silo.ServiceProvider.AddService<IDrawHandler>(drawHandler);
        Silo.ServiceProvider.AddService<IClockHandler>(clockHandler);

        _state = Silo.StorageManager.GetStorage<GameGrainState>(GameGrain.StateName).State;
        _stateStats = Silo.StorageManager.GetStorageStats(GameGrain.StateName)!;

        _whiteGameEndedStream = ProbeGameEndedStream(_whitePlayer.UserId);
        _blackGameEndedStream = ProbeGameEndedStream(_blackPlayer.UserId);
    }

    private TestStream<GameEndedEvent> ProbeGameEndedStream(string id) =>
        Silo.AddStreamProbe<GameEndedEvent>(
            id,
            streamNamespace: nameof(GameEndedEvent),
            StreamingConstants.StreamProvider
        );

    private async Task<GameGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<GameGrain>(_gameToken);

    [Fact]
    public async Task SyncRevisionAsync_calls_notifier_with_current_revision()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        ConnectionId connectionId = "test-connection";

        var result = await grain.SyncRevisionAsync(connectionId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();

        await _gameNotifierMock
            .Received(1)
            .SyncRevisionAsync(connectionId, _state.CurrentGame!.NotifierState);
    }

    [Fact]
    public async Task GetStateAsync_returns_the_correct_game_state()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.GetStateAsync();

        result.IsError.Should().BeFalse();
        ClockPlayerSnapshot expectedPlyerSnapshot = new(
            TimeLeftMs: _pool.TimeControl.BaseSeconds * 1000,
            TimeUntilAbandonMs: _settings.FirstMoveGracePeriod.TotalMilliseconds,
            IsInGracePeriod: true
        );
        ClockSnapshot expectedClock = new(
            WhiteClock: expectedPlyerSnapshot,
            BlackClock: expectedPlyerSnapshot,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds(),
            ServerTime: _fakeNow.ToUnixTimeMilliseconds(),
            IsFrozen: false
        );
        var legalMoves = _gameCore.GetLegalMoves(_state.CurrentGame!.Core);
        GameState expectedGameState = new(
            Revision: _state.CurrentGame.NotifierState.Revision,
            GameSource: _state.CurrentGame.GameSource,
            Pool: _pool,
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            Clocks: expectedClock,
            SideToMove: GameColor.White,
            InitialFen: _state.CurrentGame.InitialFen,
            MoveHistory: [],
            DrawState: new DrawState(),
            LegalMoves: legalMoves.MovePaths,
            Overtime: _overtime.ToSnapshot(_state.CurrentGame.OvertimeState)
        );
        result.Value.Should().BeEquivalentTo(expectedGameState);
    }

    [Fact]
    public async Task MovePieceAsync_ends_game_when_needed()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var whiteMove1 = (from: new AlgebraicPoint("b1"), to: new AlgebraicPoint("c3"));
        var blackMove1 = (from: new AlgebraicPoint("b10"), to: new AlgebraicPoint("c8"));

        var whiteMove2 = (from: new AlgebraicPoint("c3"), to: new AlgebraicPoint("b1"));
        var blackMove2 = (from: new AlgebraicPoint("c8"), to: new AlgebraicPoint("b10"));

        for (int i = 0; i < 3; i++)
        {
            var (whiteFrom, whiteTo) = i % 2 == 0 ? whiteMove1 : whiteMove2;
            var (blackFrom, blackTo) = i % 2 == 0 ? blackMove1 : blackMove2;

            var result1 = await grain.MovePieceAsync(
                _whitePlayer.UserId,
                new(whiteFrom, whiteTo),
                ApiTestBase.CT
            );
            var result2 = await grain.MovePieceAsync(
                _blackPlayer.UserId,
                new(blackFrom, blackTo),
                ApiTestBase.CT
            );

            result1.IsError.Should().BeFalse();
            result2.IsError.Should().BeFalse();
            _state.CurrentGame!.Result.Should().BeNull();
        }

        await grain.MovePieceAsync(
            _whitePlayer.UserId,
            new(whiteMove2.from, whiteMove2.to),
            ApiTestBase.CT
        );
        await grain.MovePieceAsync(
            _blackPlayer.UserId,
            new(blackMove2.from, blackMove2.to),
            ApiTestBase.CT
        );
        _stateStats.Writes.Should().BeGreaterThan(1);
        await TestGameEndedAsync(grain, _gameResultDescriber.ThreeFold());
    }

    [Fact]
    public async Task MovePieceAsync_reschedules_clock()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);
        await GoOutOfGracePeriodAsync(grain);

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(10));
        await MakeLegalMoveAsync(grain, _whitePlayer);

        Silo.TimerRegistry.Mock.Invocations.Clear();
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(100));
        await MakeLegalMoveAsync(grain, _blackPlayer);

        // it should now schedule to timer to WHITES clock, so base seconds - 10 + increment
        var context = Silo.GetContextFromGrain(grain);
        Silo.TimerRegistry.Mock.Verify(x =>
            x.RegisterGrainTimer(
                context,
                It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(),
                It.IsAny<It.IsAnyType>(),
                new()
                {
                    DueTime = TimeSpan.FromSeconds(
                        _pool.TimeControl.BaseSeconds - 10 + _pool.TimeControl.IncrementSeconds
                    ),
                    Period = Timeout.InfiniteTimeSpan,
                }
            )
        );
    }

    [Fact]
    public async Task RequestDrawAsync_ends_game_when_needed()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var requestResult = await grain.RequestDrawAsync(
            byUserId: _whitePlayer.UserId,
            ApiTestBase.CT
        );
        requestResult.IsError.Should().BeFalse();
        _state.CurrentGame!.Result.Should().BeNull();
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);

        var drawState = await grain.GetStateAsync();
        drawState.Value.DrawState.ActiveRequester.Should().Be(GameColor.White);

        var acceptResult = await grain.RequestDrawAsync(
            byUserId: _blackPlayer.UserId,
            ApiTestBase.CT
        );
        acceptResult.IsError.Should().BeFalse();
        await TestGameEndedAsync(grain, _gameResultDescriber.DrawByAgreement());
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task DeclineDrawAsync_declines_draw()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(byUserId: _whitePlayer.UserId, ApiTestBase.CT);
        var result = await grain.DeclineDrawAsync(byUserId: _blackPlayer.UserId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);

        var drawState = await grain.GetStateAsync();
        drawState.Value.DrawState.ActiveRequester.Should().BeNull();
    }

    [Fact]
    public async Task RequestGameEndAsync_aborts_the_game_if_not_enough_moves_have_been_made()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // No moves or just one move = still abortable
        await grain.RequestGameEndAsync(_whitePlayer.UserId, ApiTestBase.CT);

        await TestGameEndedAsync(grain, _gameResultDescriber.Aborted(GameColor.White));
    }

    [Fact]
    public async Task RequestGameEndAsync_resigns_the_game_after_abortion_threshold_is_passed()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // make enough moves to exceed abort threshold
        await MakeLegalMoveAsync(grain, _whitePlayer);
        await MakeLegalMoveAsync(grain, _blackPlayer);
        await MakeLegalMoveAsync(grain, _whitePlayer);

        _stateStats.ResetCounts();
        await grain.RequestGameEndAsync(_whitePlayer.UserId, ApiTestBase.CT);

        await TestGameEndedAsync(grain, _gameResultDescriber.Resignation(GameColor.White));
    }

    [Fact]
    public async Task OnClockTimerElapsedAsync_ends_the_game_when_time_runs_out()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain, timeControl: new(60, 0));
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + _settings.FirstMoveGracePeriod);

        await Silo.FireAllTimersAsync();

        await TestGameEndedAsync(grain, _gameResultDescriber.Aborted(GameColor.White));
    }

    [Fact]
    public async Task OnClockTimerElapsedAsync_doesnt_end_the_game_when_not_necessary()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain, timeControl: new(BaseSeconds: 10, 0));
        await GoOutOfGracePeriodAsync(grain);

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromSeconds(5));
        await Silo.FireAllTimersAsync();

        _state.CurrentGame!.Result.Should().BeNull();

        var context = Silo.GetContextFromGrain(grain);
        Silo.TimerRegistry.NumberOfActiveTimers.Should().Be(1);
        Silo.TimerRegistry.Mock.Verify(x =>
            x.RegisterGrainTimer(
                context,
                It.IsAny<Func<It.IsAnyType, CancellationToken, Task>>(),
                It.IsAny<It.IsAnyType>(),
                new() { DueTime = TimeSpan.FromSeconds(5), Period = Timeout.InfiniteTimeSpan }
            )
        );
    }

    private Move GetLegalMoves() =>
        _gameCore.GetLegalMoves(_state.CurrentGame!.Core).MoveMap.First().Value;

    private async Task<Move> MakeLegalMoveAsync(GameGrain grain, GamePlayer player)
    {
        var move = GetLegalMoves();
        await grain.MovePieceAsync(player.UserId, key: new MoveKey(move));
        return move;
    }

    private async Task StartGameAsync(
        GameGrain grain,
        GamePlayer? whitePlayer = null,
        GamePlayer? blackPlayer = null,
        TimeControlSettings? timeControl = null,
        PoolType? poolType = null
    )
    {
        await grain.StartGameAsync(
            whitePlayer: whitePlayer ?? _whitePlayer,
            blackPlayer: blackPlayer ?? _blackPlayer,
            pool: new PoolKey(
                PoolType: poolType ?? _pool.PoolType,
                TimeControl: timeControl ?? _pool.TimeControl
            ),
            GameSource.Unknown,
            ApiTestBase.CT
        );
        _stateStats.ResetCounts();
    }

    private async Task TestGameEndedAsync(GameGrain grain, GameEndStatus expectedEndStatus)
    {
        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                _gameToken,
                result: ArgEx.FluentAssert<GameResultData>(
                    (x) =>
                    {
                        x?.Result.Should().Be(expectedEndStatus.Result);
                        x?.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);
                    }
                ),
                finalClocks: _gameClock.ToSnapshot(_state.CurrentGame!.ClockState),
                _state.CurrentGame.NotifierState
            );

        _whiteGameEndedStream.VerifySend(e =>
            e.GameToken == _gameToken
            && e.EndStatus.Result == expectedEndStatus.Result
            && e.EndStatus.ResultDescription == expectedEndStatus.ResultDescription
        );
        _blackGameEndedStream.VerifySend(e =>
            e.GameToken == _gameToken
            && e.EndStatus.Result == expectedEndStatus.Result
            && e.EndStatus.ResultDescription == expectedEndStatus.ResultDescription
        );

        var gameStateResult = await grain.GetStateAsync();
        gameStateResult.IsError.Should().BeFalse();
        var gameState = gameStateResult.Value;

        gameState.ResultData.Should().NotBeNull();
        gameState.ResultData.Result.Should().Be(expectedEndStatus.Result);
        gameState.ResultData.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);

        gameState.Clocks.IsFrozen.Should().BeTrue();

        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);

        Silo.ReminderRegistry.Mock.Verify(x =>
            x.UnregisterReminder(
                Silo.GetGrainId(grain),
                It.Is<IGrainReminder>(r => r.ReminderName == GameGrain.ClockReactivationReminder)
            )
        );
    }

    private async Task GoOutOfGracePeriodAsync(GameGrain grain)
    {
        await MakeLegalMoveAsync(grain, _whitePlayer);
        await MakeLegalMoveAsync(grain, _blackPlayer);
    }
}
