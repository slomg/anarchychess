using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class OvertimeTests
{
    private readonly Overtime _overtime;

    private readonly IRandomProvider _randomMock = Substitute.For<IRandomProvider>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IPlayableMoveProvider _playableMoveProviderMock =
        Substitute.For<IPlayableMoveProvider>();
    private readonly IMoveEncoder _moveEncoderMock = Substitute.For<IMoveEncoder>();

    private readonly GameSettings _settings;
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly OvertimeState _state = new();

    public OvertimeTests()
    {
        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Game;

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _overtime = new(
            Options.Create(settings),
            _randomMock,
            _timeProviderMock,
            _playableMoveProviderMock,
            _moveEncoderMock
        );
    }

    [Fact]
    public void StartOvertimeTurn_creates_the_correct_overtime_position()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("a3"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("b5"), PieceFactory.Black(PieceType.King));

        _randomMock.Next(3).Returns(1); // pick queen
        _randomMock.Next(2).Returns(1); // pick king
        _randomMock.Next(1).Returns(0); // pick rook, but king was already picked

        ChessBoard firstExpectedBoard = new(board);
        firstExpectedBoard.RemovePiece(new("a2"));
        var firstLegalMoves = new LegalMoveSetFaker().Generate();
        byte[] firstEncoded = [1, 2, 3];
        _moveEncoderMock.EncodeMoves(firstLegalMoves.MovePaths).Returns(firstEncoded);
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(firstExpectedBoard)
            .Returns(firstLegalMoves);

        ChessBoard secondExpectedBoard = new(firstExpectedBoard);
        secondExpectedBoard.RemovePiece(new("a3"));
        var secondLegalMoves = new LegalMoveSetFaker().Generate();
        byte[] secondEncoded = [4, 5, 6];
        _moveEncoderMock.EncodeMoves(secondLegalMoves.MovePaths).Returns(secondEncoded);
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(secondExpectedBoard)
            .Returns(secondLegalMoves);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        var expectedFirstTimestamp =
            _fakeNow.ToUnixTimeMilliseconds()
            + (long)_settings.OvertimeRemovalInterval.TotalMilliseconds;
        var expectedSecondTimestamp =
            expectedFirstTimestamp + (long)_settings.OvertimeRemovalInterval.TotalMilliseconds;
        List<OvertimePendingRemovalNotification> expectedResult =
        [
            new(firstEncoded, new("a2"), expectedFirstTimestamp),
            new(secondEncoded, new("a3"), expectedSecondTimestamp),
        ];
        result.Should().BeEquivalentTo(expectedResult);

        List<PendingRemovalEntry> expectedPendingRemoval =
        [
            new(new("a2"), firstLegalMoves, expectedFirstTimestamp),
            new(new("a3"), secondLegalMoves, expectedSecondTimestamp),
        ];
        _state
            .PlayerOvertime.Should()
            .BeEquivalentTo(
                new Dictionary<GameColor, PlayerOvertime>()
                {
                    [GameColor.White] = new() { PendingRemoval = expectedPendingRemoval },
                }
            );
    }

    [Fact]
    public void StartOvertimeTurn_uses_remainderms_for_first_removal()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("a3"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("b5"), PieceFactory.Black(PieceType.King));

        long remainderMs = 400;
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval = [],
            RemainderMs = remainderMs,
        };

        var firstLegalMoves = new LegalMoveSetFaker().Generate();
        var secondLegalMoves = new LegalMoveSetFaker().Generate();
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(Arg.Any<ChessBoard>())
            .Returns(firstLegalMoves, secondLegalMoves);
        _moveEncoderMock.EncodeMoves(Arg.Any<IReadOnlyList<MovePath>>()).Returns([1, 2]);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        long nowMs = _fakeNow.ToUnixTimeMilliseconds();
        long expectedFirstTimestamp =
            nowMs - remainderMs + (long)_settings.OvertimeRemovalInterval.TotalMilliseconds;
        long expectedSecondTimestamp =
            expectedFirstTimestamp + (long)_settings.OvertimeRemovalInterval.TotalMilliseconds;

        result[0].RemoveAtTimestamp.Should().Be(expectedFirstTimestamp);
        result[1].RemoveAtTimestamp.Should().Be(expectedSecondTimestamp);
    }

    [Fact]
    public void StartOvertimeTurn_never_picks_king_first()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("a3"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("b5"), PieceFactory.Black(PieceType.King));

        // first would be the king, but the king shouldn't be picked first
        _randomMock.Next(Arg.Any<int>()).Returns(0);

        var legalMoves = new LegalMoveSetFaker().Generate();
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(Arg.Any<ChessBoard>())
            .Returns(legalMoves);
        _moveEncoderMock.EncodeMoves(Arg.Any<IReadOnlyList<MovePath>>()).Returns([1]);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().NotBeEmpty();
        result[0].RemoveFrom.Should().NotBe(new AlgebraicPoint("a1"));
    }

    [Fact]
    public void StartOvertimeTurn_picks_king_first_if_it_is_the_only_piece()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("d1"), PieceFactory.Black(PieceType.King));

        _randomMock.Next(1).Returns(0);

        var legalMoves = new LegalMoveSetFaker().Generate();
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(Arg.Any<IReadOnlyChessBoard>())
            .Returns(legalMoves);

        _moveEncoderMock.EncodeMoves(Arg.Any<IReadOnlyList<MovePath>>()).Returns([42]);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().HaveCount(1);
        result[0].RemoveFrom.Should().Be(new AlgebraicPoint("e1"));
    }

    [Fact]
    public void ToSnapshot_creates_the_right_snapshot_when_there_are_no_pending_positions()
    {
        var result = _overtime.ToSnapshot(_state);

        result
            .Should()
            .BeEquivalentTo(new OvertimeSnapshot(WhiteOvertime: null, BlackOvertime: null));
    }

    [Fact]
    public void ToSnapshot_creates_the_right_snapshot_with_pending_positions()
    {
        PendingRemovalEntry white1 = new(
            new("a1"),
            new LegalMoveSetFaker().Generate(),
            RemoveAtTimestamp: 1234
        );
        PendingRemovalEntry white2 = new(
            new("c5"),
            new LegalMoveSetFaker().Generate(),
            RemoveAtTimestamp: 4567
        );
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval = [white1, white2],
            RemainderMs = 8910,
        };

        PendingRemovalEntry black1 = new(
            new("f6"),
            new LegalMoveSetFaker().Generate(),
            RemoveAtTimestamp: 1112
        );
        PendingRemovalEntry black2 = new(
            new("g7"),
            new LegalMoveSetFaker().Generate(),
            RemoveAtTimestamp: 1314
        );
        _state.PlayerOvertime[GameColor.Black] = new()
        {
            PendingRemoval = [black1, black2],
            RemainderMs = 5678,
        };

        var result = _overtime.ToSnapshot(_state);

        result
            .Should()
            .BeEquivalentTo(
                new OvertimeSnapshot(
                    WhiteOvertime:
                    [
                        new PendingOvertimeRemovalPathSnapshot(
                            white1.LegalMoves.MovePaths,
                            white1.RemoveFrom,
                            white1.RemoveAtTimestamp
                        ),
                        new PendingOvertimeRemovalPathSnapshot(
                            white2.LegalMoves.MovePaths,
                            white2.RemoveFrom,
                            white2.RemoveAtTimestamp
                        ),
                    ],
                    BlackOvertime:
                    [
                        new PendingOvertimeRemovalPathSnapshot(
                            black1.LegalMoves.MovePaths,
                            black1.RemoveFrom,
                            black1.RemoveAtTimestamp
                        ),
                        new PendingOvertimeRemovalPathSnapshot(
                            black2.LegalMoves.MovePaths,
                            black2.RemoveFrom,
                            black2.RemoveAtTimestamp
                        ),
                    ]
                )
            );
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_returns_empty_if_no_pending_removals()
    {
        var (positions, newLegalMoves) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().BeEmpty();
        newLegalMoves.Should().BeNull();
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_removes_correct_number_of_positions()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();

        PendingRemovalEntry pending1 = new(
            new("a1"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 500
        );
        PendingRemovalEntry pending2 = new(
            new("b2"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 1000
        );
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [pending1, pending2] };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(600));

        var (positions, newLegalMoves) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().BeEquivalentTo([pending1.RemoveFrom]);
        newLegalMoves.Should().BeEquivalentTo(pending1.LegalMoves);
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_returns_all_positions_and_game_over_if_time_exceeds()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();

        PendingRemovalEntry pending1 = new(
            new("c3"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 100
        );
        PendingRemovalEntry pending2 = new(
            new("d4"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 200
        );
        _state.PlayerOvertime[GameColor.Black] = new() { PendingRemoval = [pending1, pending2] };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(300));

        var (positions, newLegalMoves) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.Black,
            _state
        );

        positions.Should().BeEquivalentTo([pending1.RemoveFrom, pending2.RemoveFrom]);
        newLegalMoves.Should().BeEquivalentTo(pending2.LegalMoves);
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_returns_nothing_if_ran_before_any_pending()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();

        PendingRemovalEntry pending1 = new(
            new("c3"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 100
        );
        PendingRemovalEntry pending2 = new(
            new("d4"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 200
        );
        _state.PlayerOvertime[GameColor.Black] = new() { PendingRemoval = [pending1, pending2] };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(50));

        var (positions, newLegalMoves) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.Black,
            _state
        );

        positions.Should().BeEmpty();
        newLegalMoves.Should().BeNull();
    }

    [Fact]
    public void ConsumeOvertimeRemovals_returns_same_as_GetRemovedPiecesSinceLastMove()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();
        PendingRemovalEntry pending1 = new(
            new("a1"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 100
        );
        PendingRemovalEntry pending2 = new(
            new("b2"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 200
        );
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [pending1, pending2] };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(150));

        var resultQuery = _overtime.GetRemovedPiecesSinceLastMove(GameColor.White, _state);
        var resultProcess = _overtime.ConsumeOvertimeRemovals(GameColor.White, _state);

        resultProcess.Should().BeEquivalentTo(resultQuery);
        _state.PlayerOvertime[GameColor.White].PendingRemoval.Should().Equal([pending2]);
    }

    [Fact]
    public void ConsumeOvertimeRemovals_removes_all_when_time_exceeds()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();
        PendingRemovalEntry pending1 = new(
            new("c3"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 100
        );
        PendingRemovalEntry pending2 = new(
            new("d4"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 200
        );
        _state.PlayerOvertime[GameColor.Black] = new()
        {
            PendingRemoval = [pending1, pending2],
            RemainderMs = 1234,
        };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(300));

        var (pendingRemoval, _) = _overtime.ConsumeOvertimeRemovals(GameColor.Black, _state);

        pendingRemoval.Should().BeEquivalentTo([pending1.RemoveFrom, pending2.RemoveFrom]);
        _state.PlayerOvertime[GameColor.Black].PendingRemoval.Should().BeEmpty();
        _state.PlayerOvertime[GameColor.Black].RemainderMs.Should().Be(0);
    }

    [Fact]
    public void ConsumeOvertimeRemovals_handles_no_pending_removals_gracefully()
    {
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [] };

        _overtime.ConsumeOvertimeRemovals(GameColor.White, _state);

        _state.PlayerOvertime[GameColor.White].PendingRemoval.Should().BeEmpty();
    }

    [Fact]
    public void ConsumeOvertimeRemovals_sets_remainder_for_next_pending_removal()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();

        PendingRemovalEntry pending1 = new(
            new("a1"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 500
        );
        var pending2 = new PendingRemovalEntry(
            new("b2"),
            new LegalMoveSetFaker().Generate(),
            nowMs + 1000
        );
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [pending1, pending2] };

        // advance time past the first removal only
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(600));

        var (pendingRemoval, _) = _overtime.ConsumeOvertimeRemovals(GameColor.White, _state);

        pendingRemoval.Should().BeEquivalentTo([pending1.RemoveFrom]);
        _state.PlayerOvertime[GameColor.White].PendingRemoval.Should().Equal([pending2]);

        // RemainderMs should be time until next removal
        long expectedRemainder =
            pending2.RemoveAtTimestamp - _fakeNow.AddMilliseconds(600).ToUnixTimeMilliseconds();
        _state.PlayerOvertime[GameColor.White].RemainderMs.Should().Be(expectedRemainder);
    }

    [Fact]
    public void HasStartedOvertime_returns_false_if_no_pending_removals()
    {
        bool result = _overtime.HasStartedOvertime(GameColor.White, _state);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasStartedOvertime_returns_true_if_pending_removals_exist()
    {
        _state.PlayerOvertime[GameColor.Black] = new()
        {
            PendingRemoval =
            [
                new(new("g7"), new LegalMoveSetFaker().Generate(), RemoveAtTimestamp: 1234),
            ],
        };

        bool result = _overtime.HasStartedOvertime(GameColor.Black, _state);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetTimeUntilDefeat_returns_zero_if_color_key_not_present()
    {
        var result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        result.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetTimeUntilDefeat_returns_zero_if_pending_removals_empty()
    {
        _state.PlayerOvertime[GameColor.White] = new()
        {
            RemainderMs = 500, // remainder shouldn't matter
            PendingRemoval = [],
        };

        TimeSpan result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        result.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetTimeUntilDefeat_returns_correct_time()
    {
        long nowMs = _fakeNow.ToUnixTimeMilliseconds();

        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval =
            [
                new(new("a1"), new LegalMoveSetFaker().Generate(), nowMs + 1000),
                new(new("b2"), new LegalMoveSetFaker().Generate(), nowMs + 2000),
            ],
        };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(500));

        TimeSpan result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        // 2000 - 500 = 1500 ms remaining
        result.Should().Be(TimeSpan.FromMilliseconds(1500));
    }

    [Fact]
    public void EndOvertime_clears_overtime()
    {
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval =
            [
                new(new("a1"), new LegalMoveSetFaker().Generate(), 123),
                new(new("b2"), new LegalMoveSetFaker().Generate(), 456),
            ],
        };
        _state.PlayerOvertime[GameColor.Black] = new()
        {
            PendingRemoval =
            [
                new(new("a2"), new LegalMoveSetFaker().Generate(), 789),
                new(new("b3"), new LegalMoveSetFaker().Generate(), 101),
            ],
        };

        _overtime.EndOvertime(_state);

        _state.PlayerOvertime.Should().BeEmpty();
    }
}
