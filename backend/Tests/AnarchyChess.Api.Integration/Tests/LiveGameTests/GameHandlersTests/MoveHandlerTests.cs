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
    private readonly IGameCore _gameCore;
    private readonly IGameResultDescriber _gameResultDescriber;

    private readonly GameToken _gameToken = "testtoken";

    private readonly IGameNotifier _notifierMock = Substitute.For<IGameNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly GameData _gameData;

    public MoveHandlerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameCore = Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _fenEncoder = Scope.ServiceProvider.GetRequiredService<IFenEncoder>();
        _sanCalculator = Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
        _gameResultDescriber = Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();

        var settings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
        _settings = settings.Value.Game;

        _clock = new(settings, _timeProviderMock);
        _overtime = new(
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
        _gameData = GameUtils.CreateGameData(_gameCore, _clock);
    }

    [Fact]
    public async Task HandleMoveAsync_with_a_valid_move_creates_a_correct_move_made_notification()
    {
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _gameCore, _gameToken, _gameData);
        _notifierMock.ClearReceivedCalls();

        var in2Seconds = _fakeNow + TimeSpan.FromSeconds(2);
        _timeProviderMock.GetUtcNow().Returns(in2Seconds);

        var move = GameUtils.GetLegalMove(_gameCore, _gameData);
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

        var legalMoves = _gameCore.GetLegalMoves(_gameData.Core);
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
                                EncodedLegalMoves: _gameCore.EncodeLegalMoves(_gameData.Core),
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
        var gameData = GameUtils.CreateGameData(_gameCore, _clock, board: board);

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
            new MoveKey(GameUtils.GetLegalMove(_gameCore, _gameData)),
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
            new MoveKey(GameUtils.GetLegalMove(_gameCore, _gameData)),
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
    public async Task HandleMoveAsync_starts_overtime_turn_for_next_player()
    {
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _gameCore, _gameToken, _gameData);

        _timeProviderMock
            .GetUtcNow()
            .Returns(_fakeNow.AddSeconds(_gameData.Pool.TimeControl.BaseSeconds));

        await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_gameCore, _gameData)),
            _gameToken,
            _gameData,
            CT
        );
        await _handler.HandleMoveAsync(
            _gameData.Players.BlackPlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_gameCore, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        _overtime.HasStartedOvertime(GameColor.White, _gameData.OvertimeState).Should().BeTrue();
        await _notifierMock
            .Received(1)
            .NotifyOvertimeAsync(
                GameColor.White,
                overtimeTurnStartedAt: _overtime.GetOvertimeTurnStartedAt(_gameData.OvertimeState),
                secondRemainder: _overtime.GetPlayerSecondRemainderMs(
                    GameColor.White,
                    _gameData.OvertimeState
                ),
                Arg.Is<List<OvertimePendingRemovalNotification>>(x => x.Count > 1),
                _gameToken,
                _gameData.NotifierState
            );
    }

    [Fact]
    public async Task HandleMoveAsync_removes_overtime_pieces_for_current_player()
    {
        await GameUtils.GoOutOfGracePeriodAsync(_handler, _gameCore, _gameToken, _gameData);
        var now = _fakeNow.AddSeconds(_gameData.Pool.TimeControl.BaseSeconds);
        _timeProviderMock.GetUtcNow().Returns(now);

        await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_gameCore, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        // black makes a move, it's now white's turn, timeout should start now
        await _handler.HandleMoveAsync(
            _gameData.Players.BlackPlayer.UserId,
            new MoveKey(GameUtils.GetLegalMove(_gameCore, _gameData)),
            _gameToken,
            _gameData,
            CT
        );

        now += TimeSpan.FromSeconds(2);
        _timeProviderMock.GetUtcNow().Returns(now);
        var (pendingRemoval, newLegalMoves, _) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _gameData.OvertimeState
        );
        HashSet<AlgebraicPoint> pendingRemovalPoints = [];
        // white just played the move before timeout, so no piece should've been removed
        foreach (var point in pendingRemoval)
        {
            _gameData.Core.Board.IsEmpty(point).Should().BeFalse();
            pendingRemovalPoints.Add(point);
        }
        var beforePendingLength = _gameData
            .OvertimeState
            .PlayerOvertime[GameColor.White]
            .PendingRemoval
            .Count;

        await _handler.HandleMoveAsync(
            _gameData.Players.WhitePlayer.UserId,
            new MoveKey(
                // make sure the move is not interfering with the result
                newLegalMoves.MoveMap.Values.First(move =>
                    !pendingRemoval.Contains(move.From) && !pendingRemoval.Contains(move.To)
                )
            ),
            _gameToken,
            _gameData,
            CT
        );

        foreach (var point in pendingRemoval)
        {
            _gameData.Core.Board.IsEmpty(point).Should().BeTrue();
        }
        // next legal moves != next legal moves if it was still white's turn
        _gameCore.GetLegalMoves(_gameData.Core).Should().NotBeEquivalentTo(newLegalMoves);
        _gameData
            .OvertimeState.PlayerOvertime[GameColor.White]
            .PendingRemoval.Count.Should()
            .Be(beforePendingLength - 2);
    }
}
