using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.SanNotation;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests.GameHandlersTests;

public class MoveHandlerTests : BaseIntegrationTest
{
    private readonly MoveHandler _handler;

    private readonly GameSettings _settings;
    private readonly GameClock _clock;
    private readonly Overtime _overtime;
    private readonly IFenEncoder _fenEncoder;
    private readonly ISanCalculator _sanCalculator;
    private readonly IGameCore _core;
    private readonly IGameResultDescriber _gameResultDescriber;

    private readonly GameToken _gameToken = "testtoken";

    private readonly IGameNotifier _notifierMock = Substitute.For<IGameNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly GameData _gameData;

    public MoveHandlerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _core = Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _fenEncoder = Scope.ServiceProvider.GetRequiredService<IFenEncoder>();
        _sanCalculator = Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
        _gameResultDescriber = Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();

        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Game;

        _clock = new(settings, _timeProviderMock);
        _overtime = new(
            settings,
            Scope.ServiceProvider.GetRequiredService<IRandomProvider>(),
            _timeProviderMock,
            Scope.ServiceProvider.GetRequiredService<IPlayableMoveProvider>(),
            Scope.ServiceProvider.GetRequiredService<IMoveEncoder>()
        );

        _handler = new MoveHandler(
            Scope.ServiceProvider.GetRequiredService<ILogger<MoveHandler>>(),
            Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>(),
            Scope.ServiceProvider.GetRequiredService<IGameCore>(),
            _clock,
            _notifierMock,
            _overtime
        );

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        _gameData = GameUtils.CreateGameData(_core, _clock);
    }

    [Fact]
    public async Task HandleMoveAsync_with_a_valid_move_creates_a_correct_move_made_notification()
    {
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _core, _gameToken, _gameData);
        _notifierMock.ClearReceivedCalls();

        var in2Seconds = _fakeNow + TimeSpan.FromSeconds(2);
        _timeProviderMock.GetUtcNow().Returns(in2Seconds);

        var move = GameUtils.GetLegalMove(_core, _gameData);
        var result = await _handler.HandleMoveAsync(
            moveMadeBy: _gameData.Players.WhitePlayer.UserId,
            new MoveKey(move),
            _gameToken,
            _gameData,
            CT
        );

        var expectedTimeLeft =
            _gameData.Pool.TimeControl.BaseSeconds * 1000
            + _gameData.Pool.TimeControl.IncrementSeconds * 1000 // add increment
            - 2 * 1000; // removed elapsed time

        var legalMoves = _core.GetLegalMoves(_gameData.Core);
        MoveSnapshot expectedMoveSnapshot = new(
            Path: MovePath.FromMove(move, GameLogicConstants.BoardWidth),
            Fen: _fenEncoder.EncodeFen(_gameData.Core.Board).FullFen,
            NextSideToMove: GameColor.Black,
            San: _sanCalculator.CalculateSan(move, legalMoves.AllMoves),
            TimeLeft: expectedTimeLeft
        );
        ClockSnapshot expectedClock = new(
            WhiteClock: new(expectedTimeLeft, TimeUntilAbandonMs: null, IsInGracePeriod: false),
            BlackClock: new(
                _gameData.Pool.TimeControl.BaseSeconds * 1000,
                TimeUntilAbandonMs: null,
                IsInGracePeriod: false
            ),
            LastUpdated: in2Seconds.ToUnixTimeMilliseconds(),
            ServerTime: in2Seconds.ToUnixTimeMilliseconds(),
            IsFrozen: false
        );
        await _notifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                notification: ArgEx.FluentAssert<MoveNotification>(x =>
                    x.Should()
                        .BeEquivalentTo(
                            new MoveNotification(
                                GameToken: _gameToken,
                                Move: expectedMoveSnapshot,
                                PlyNumber: 3,
                                Clocks: expectedClock,
                                SideToMoveUserId: _gameData.Players.BlackPlayer.UserId,
                                EncodedLegalMoves: _core.EncodeLegalMoves(_gameData.Core),
                                DidMoveEndGame: false
                            )
                        )
                ),
                _gameData.NotifierState
            );
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleMoveAsync_with_an_invalid_move_should_returns_an_error()
    {
        var result = await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(from: new AlgebraicPoint("e2"), to: new AlgebraicPoint("e8")),
            _gameToken,
            _gameData,
            CT
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public async Task HandleMoveAsync_that_results_in_game_over_ends_the_game()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("a3"), PieceFactory.Black(PieceType.King));
        board.PlacePiece(new("b1"), PieceFactory.White(PieceType.King));
        var gameData = GameUtils.CreateGameData(_core, _clock, board: board);

        var result = await _handler.HandleMoveAsync(
            gameData.Players.WhitePlayer.UserId,
            new MoveKey(from: new AlgebraicPoint("a1"), to: new AlgebraicPoint("a3")),
            _gameToken,
            gameData,
            CT
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(_gameResultDescriber.KingCaptured(by: GameColor.White));
        await _notifierMock
            .ReceivedWithAnyArgs(1)
            .NotifyMoveMadeAsync(Arg.Is<MoveNotification>(x => x.DidMoveEndGame == true), default!);
    }

    [Fact]
    public async Task HandleMoveAsync_decrements_draw_cooldown()
    {
        var drawCooldown = _settings.DrawCooldown;
        _gameData.DrawRequest.RequestDraw(GameColor.White);
        _gameData.DrawRequest.TryDeclineDraw(GameColor.Black, drawCooldown);

        var result = await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_core, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        await _notifierMock
            .DidNotReceiveWithAnyArgs()
            .NotifyDrawStateChangeAsync(default, default!, default!);

        var drawState = _gameData.DrawRequest.GetState();
        drawState.WhiteCooldown.Should().Be(drawCooldown - 1);
        drawState.BlackCooldown.Should().Be(0);
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleMoveAsync_declines_pending_draw_request()
    {
        _gameData.DrawRequest.RequestDraw(GameColor.Black);

        var result = await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_core, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        await _notifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                _gameToken,
                new DrawState(BlackCooldown: _settings.DrawCooldown),
                _gameData.NotifierState
            );

        _gameData.DrawRequest.ActiveRequester.Should().BeNull();
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleMoveAsync_starts_overtime_for_next_player_if_they_are_timed_out()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("b1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("h8"), PieceFactory.Black(PieceType.King));
        var gameData = GameUtils.CreateGameData(_core, _clock, board: board);
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _core, _gameToken, gameData);

        var timeoutTime = _fakeNow + TimeSpan.FromSeconds(gameData.Pool.TimeControl.BaseSeconds);
        _timeProviderMock.GetUtcNow().Returns(timeoutTime);

        await _handler.HandleMoveAsync(
            gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_core, gameData)),
            _gameToken,
            gameData,
            CT
        );

        await _handler.HandleMoveAsync(
            gameData.Players.BlackPlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_core, gameData)),
            _gameToken,
            gameData,
            CT
        );

        _overtime.HasEnteredOvertime(GameColor.White, gameData.OvertimeState).Should().BeTrue();
        _overtime.HasEnteredOvertime(GameColor.Black, gameData.OvertimeState).Should().BeFalse();

        gameData
            .OvertimeState.LastMoveAtTimestamp.Should()
            .Be(timeoutTime.ToUnixTimeMilliseconds());
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
    public async Task HandleMoveAsync_does_not_start_overtime_if_next_player_is_not_timed_out()
    {
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _core, _gameToken, _gameData);

        await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_core, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        _overtime.HasEnteredOvertime(GameColor.White, _gameData.OvertimeState).Should().BeFalse();
        _overtime.HasEnteredOvertime(GameColor.Black, _gameData.OvertimeState).Should().BeFalse();
    }

    [Fact]
    public async Task HandleMoveAsync_ends_overtime_turn_and_sets_remainder()
    {
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _core, _gameToken, _gameData);

        // force white into overtime before move
        _overtime.StartOvertimeTurn(GameColor.White, _gameData.Core.Board, _gameData.OvertimeState);

        _timeProviderMock
            .GetUtcNow()
            .Returns(
                _fakeNow
                    .Add(_settings.OvertimeRemovalInterval)
                    .Add(_settings.OvertimeRemovalInterval / 2)
            );

        await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_core, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        _gameData
            .OvertimeState.PlayerOvertime[GameColor.White]
            .Should()
            .Be(_settings.OvertimeRemovalInterval / 2);
    }
}
