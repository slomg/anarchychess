using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
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

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private readonly OvertimeState _state = new();

    public OvertimeTests()
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _overtime = new(
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

        List<OvertimePendingRemovalNotification> expectedResult =
        [
            new(firstEncoded, new("a2")),
            new(secondEncoded, new("a3")),
        ];
        result.Should().BeEquivalentTo(expectedResult);
        _state.OvertimeTurnStartedAt.Should().Be(_fakeNow.ToUnixTimeMilliseconds());
        List<PendingRemovalEntry> expectedPendingRemoval =
        [
            new(new("a2"), firstLegalMoves),
            new(new("a3"), secondLegalMoves),
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
        result[0].RemovePieceAt.Should().NotBe(new AlgebraicPoint("a1"));
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
        result[0].RemovePieceAt.Should().Be(new AlgebraicPoint("e1"));
    }

    [Fact]
    public void ToSnapshot_creates_the_right_snapshot_when_there_are_no_pending_positions()
    {
        var result = _overtime.ToSnapshot(_state);

        result
            .Should()
            .BeEquivalentTo(
                new OvertimeSnapshot(
                    WhiteOvertime: null,
                    BlackOvertime: null,
                    OvertimeTurnStartedAt: 0
                )
            );
    }

    [Fact]
    public void ToSnapshot_creates_the_right_snapshot_with_pending_positions()
    {
        PendingRemovalEntry white1 = new(new("a1"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry white2 = new(new("c5"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval = [white1, white2],
            SecondRemainderMs = 1234,
        };

        PendingRemovalEntry black1 = new(new("f6"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry black2 = new(new("g7"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.Black] = new()
        {
            PendingRemoval = [black1, black2],
            SecondRemainderMs = 5678,
        };
        _state.OvertimeTurnStartedAt = 123456;

        var result = _overtime.ToSnapshot(_state);

        result
            .Should()
            .BeEquivalentTo(
                new OvertimeSnapshot(
                    WhiteOvertime: new(
                        SecondRemainderMs: 1234,
                        PendingRemoval:
                        [
                            new PendingOvertimeRemovalPathSnapshot(
                                white1.LegalMoves.MovePaths,
                                white1.Position
                            ),
                            new PendingOvertimeRemovalPathSnapshot(
                                white2.LegalMoves.MovePaths,
                                white2.Position
                            ),
                        ]
                    ),
                    BlackOvertime: new(
                        SecondRemainderMs: 5678,
                        PendingRemoval:
                        [
                            new PendingOvertimeRemovalPathSnapshot(
                                black1.LegalMoves.MovePaths,
                                black1.Position
                            ),
                            new PendingOvertimeRemovalPathSnapshot(
                                black2.LegalMoves.MovePaths,
                                black2.Position
                            ),
                        ]
                    ),
                    OvertimeTurnStartedAt: 123456
                )
            );
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_returns_empty_if_no_pending_removals()
    {
        var (positions, newLegalMoves, isGameOver) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().BeEmpty();
        newLegalMoves.MovePaths.Should().BeEmpty();
        isGameOver.Should().BeFalse();
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_removes_correct_number_of_positions()
    {
        PendingRemovalEntry pos1 = new(new("a1"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry pos2 = new(new("b2"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [pos1, pos2] };
        _state.OvertimeTurnStartedAt = _fakeNow.ToUnixTimeMilliseconds();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(1500));

        var (positions, newLegalMoves, isGameOver) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().BeEquivalentTo([pos1.Position]);
        newLegalMoves.Should().BeEquivalentTo(pos1.LegalMoves);
        isGameOver.Should().BeFalse();
        _state.PlayerOvertime[GameColor.White].SecondRemainderMs.Should().Be(500);
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_returns_all_positions_and_game_over_if_time_exceeds()
    {
        PendingRemovalEntry pos1 = new(new("c3"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry pos2 = new(new("d4"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.Black] = new() { PendingRemoval = [pos1, pos2] };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(5));

        var (positions, newLegalMoves, isGameOver) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.Black,
            _state
        );

        positions.Should().BeEquivalentTo([pos1.Position, pos2.Position]);
        newLegalMoves.Should().BeEquivalentTo(new LegalMoveSet());
        isGameOver.Should().BeTrue();
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_handles_exactly_pending_count()
    {
        PendingRemovalEntry pos1 = new(new("e5"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry pos2 = new(new("f6"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [pos1, pos2] };

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(2));

        var (positions, newLegalMoves, isGameOver) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().Equal([pos1.Position, pos2.Position]);
        newLegalMoves.MovePaths.Should().BeEmpty();
        isGameOver.Should().BeTrue();
    }

    [Fact]
    public void GetRemovedPiecesSinceLastMove_includes_remainder_in_elapsed_time()
    {
        PendingRemovalEntry pos1 = new(new("a1"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry pos2 = new(new("b2"), new LegalMoveSetFaker().Generate());

        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval = [pos1, pos2],
            SecondRemainderMs = 400,
        };

        _state.OvertimeTurnStartedAt = _fakeNow.ToUnixTimeMilliseconds();

        // 600 ms since last move + 400 ms remainder = 1000 ms
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(600));

        var (positions, newLegalMoves, isGameOver) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().Equal([pos1.Position]);
        newLegalMoves.Should().BeEquivalentTo(pos1.LegalMoves);
        isGameOver.Should().BeFalse();

        _state.PlayerOvertime[GameColor.White].SecondRemainderMs.Should().Be(0);
    }

    [Fact]
    public void ProcessOvertimeRemovals_returns_same_as_GetRemovedPiecesSinceLastMove()
    {
        var pending1 = new PendingRemovalEntry(new("a1"), new LegalMoveSetFaker().Generate());
        var pending2 = new PendingRemovalEntry(new("b2"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [pending1, pending2] };
        _state.OvertimeTurnStartedAt = _fakeNow.ToUnixTimeMilliseconds();
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(1));

        var resultQuery = _overtime.GetRemovedPiecesSinceLastMove(GameColor.White, _state);
        var resultProcess = _overtime.ProcessOvertimeRemovals(GameColor.White, _state);

        resultProcess.Should().BeEquivalentTo(resultQuery);

        _state.PlayerOvertime[GameColor.White].PendingRemoval.Should().Equal([pending2]);
    }

    [Fact]
    public void ProcessOvertimeRemovals_removes_all_when_time_exceeds()
    {
        var pending1 = new PendingRemovalEntry(new("c3"), new LegalMoveSetFaker().Generate());
        var pending2 = new PendingRemovalEntry(new("d4"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.Black] = new() { PendingRemoval = [pending1, pending2] };
        _state.OvertimeTurnStartedAt = _fakeNow.ToUnixTimeMilliseconds();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddSeconds(5));

        var (pendingRemoval, _, isGameOver) = _overtime.ProcessOvertimeRemovals(
            GameColor.Black,
            _state
        );

        isGameOver.Should().BeTrue();
        pendingRemoval.Should().BeEquivalentTo([pending1.Position, pending2.Position]);

        _state.PlayerOvertime[GameColor.Black].PendingRemoval.Should().BeEmpty();
    }

    [Fact]
    public void ProcessOvertimeRemovals_handles_no_pending_removals_gracefully()
    {
        _state.PlayerOvertime[GameColor.White] = new() { PendingRemoval = [] };
        _state.OvertimeTurnStartedAt = _fakeNow.ToUnixTimeMilliseconds();

        _overtime.ProcessOvertimeRemovals(GameColor.White, _state);

        _state.PlayerOvertime[GameColor.White].PendingRemoval.Should().BeEmpty();
    }

    [Fact]
    public void GetOvertimeTurnStartedAt_returns_correct_time()
    {
        _state.OvertimeTurnStartedAt = 123456;

        var result = _overtime.GetOvertimeTurnStartedAt(_state);

        result.Should().Be(123456);
    }

    [Fact]
    public void GetPlayerSecondRemainderMs_returns_correct_time()
    {
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval = [],
            SecondRemainderMs = 0.123,
        };

        var result = _overtime.GetPlayerSecondRemainderMs(GameColor.White, _state);

        result.Should().Be(0.123);
    }

    [Fact]
    public void GetPlayerSecondRemainderMs_returns_zero_when_no_overtime()
    {
        var result = _overtime.GetPlayerSecondRemainderMs(GameColor.White, _state);

        result.Should().Be(0);
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
            PendingRemoval = [new(new("g7"), new LegalMoveSetFaker().Generate())],
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
            SecondRemainderMs = 500, // remainder shouldn't matter
            PendingRemoval = [],
        };

        TimeSpan result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        result.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetTimeUntilDefeat_returns_seconds_correctly_for_pending_removals()
    {
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval =
            [
                new(new("a1"), new LegalMoveSetFaker().Generate()),
                new(new("b2"), new LegalMoveSetFaker().Generate()),
            ],
        };

        TimeSpan result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        result.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void GetTimeUntilDefeat_subtracts_fractional_second_remainder()
    {
        _state.PlayerOvertime[GameColor.White] = new()
        {
            SecondRemainderMs = 300,
            PendingRemoval =
            [
                new(new("a1"), new LegalMoveSetFaker().Generate()),
                new(new("b2"), new LegalMoveSetFaker().Generate()),
                new(new("c3"), new LegalMoveSetFaker().Generate()),
            ],
        };

        TimeSpan result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        result.Should().Be(TimeSpan.FromSeconds(2.7));
    }
}
