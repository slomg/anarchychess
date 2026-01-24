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

        List<OvertimePosition> expectedResult =
        [
            new(firstEncoded, new("a2")),
            new(secondEncoded, new("a3")),
        ];
        result.Should().BeEquivalentTo(expectedResult);
        _state.LastMoveAtMs.Should().Be(_fakeNow.ToUnixTimeMilliseconds());
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
        result[0].RemovedPiece.Should().NotBe(new AlgebraicPoint("a1"));
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
        result[0].RemovedPiece.Should().Be(new AlgebraicPoint("e1"));
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
        PendingRemovalEntry white1 = new(new("a1"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry white2 = new(new("c5"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.White] = new()
        {
            PendingRemoval = [white1, white2],
            SecondRemainder = 0.123,
        };

        PendingRemovalEntry black1 = new(new("f6"), new LegalMoveSetFaker().Generate());
        PendingRemovalEntry black2 = new(new("g7"), new LegalMoveSetFaker().Generate());
        _state.PlayerOvertime[GameColor.Black] = new()
        {
            PendingRemoval = [black1, black2],
            SecondRemainder = 0.456,
        };

        var result = _overtime.ToSnapshot(_state);

        result
            .Should()
            .BeEquivalentTo(
                new OvertimeSnapshot(
                    WhiteOvertime: new(
                        SecondRemainder: 0.123,
                        PendingRemoval:
                        [
                            new EncodedPendingOvertimeRemovalSnapshot(
                                white1.LegalMoves.MovePaths,
                                white1.Position
                            ),
                            new EncodedPendingOvertimeRemovalSnapshot(
                                white2.LegalMoves.MovePaths,
                                white2.Position
                            ),
                        ]
                    ),
                    BlackOvertime: new(
                        SecondRemainder: 0.456,
                        PendingRemoval:
                        [
                            new EncodedPendingOvertimeRemovalSnapshot(
                                black1.LegalMoves.MovePaths,
                                black1.Position
                            ),
                            new EncodedPendingOvertimeRemovalSnapshot(
                                black2.LegalMoves.MovePaths,
                                black2.Position
                            ),
                        ]
                    )
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
        _state.LastMoveAtMs = _fakeNow.ToUnixTimeMilliseconds();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(1500));

        var (positions, newLegalMoves, isGameOver) = _overtime.GetRemovedPiecesSinceLastMove(
            GameColor.White,
            _state
        );

        positions.Should().BeEquivalentTo([pos1.Position]);
        newLegalMoves.Should().BeEquivalentTo(pos2.LegalMoves);
        isGameOver.Should().BeFalse();
        _state.PlayerOvertime[GameColor.White].SecondRemainder.Should().Be(0.5);
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
            SecondRemainder = 0.5f, // remainder shouldn't matter
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
            SecondRemainder = 0.3,
            PendingRemoval =
            [
                new(new("a1"), new LegalMoveSetFaker().Generate()),
                new(new("b2"), new LegalMoveSetFaker().Generate()),
                new(new("c3"), new LegalMoveSetFaker().Generate()),
            ],
        };

        TimeSpan result = _overtime.GetTimeUntilDefeat(GameColor.White, _state);

        result.TotalSeconds.Should().BeApproximately(2.7, 0.001); // 3 - 0.3 // 3 - 0.3
    }
}
