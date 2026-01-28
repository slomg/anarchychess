using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
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

        _playableMoveProviderMock
            .CalculateAllPlayableMoves(Arg.Any<IReadOnlyChessBoard>())
            .Returns(new LegalMoveSetFaker().Generate());
    }

    private void MockRandom(ChessBoard board, params AlgebraicPoint[] points)
    {
        (AlgebraicPoint, Piece)[] results =
        [
            .. points.Select(point => (point, board.PeekPieceAt(point)!)),
        ];

        _randomMock
            .NextItemWeighted(
                Arg.Any<IEnumerable<(AlgebraicPoint Position, Piece Occupant)>>(),
                Arg.Any<Func<(AlgebraicPoint Position, Piece Occupant), int>>()
            )
            .Returns(results[0], results[1..]);
    }

    private PlayerOvertime CreatePlayerOvertime(
        TimeSpan? remainder = null,
        TimeSpan? removalInterval = null,
        NextOvertimeRemoval? pickedNextRemoval = null
    ) =>
        new()
        {
            Remainder = remainder ?? TimeSpan.Zero,
            RemovalInterval = removalInterval ?? _settings.OvertimeInitialRemovalInterval,
            PickedNextRemoval = pickedNextRemoval,
        };

    [Fact]
    public void GetNextRemoval_returns_game_over_if_player_has_no_pieces()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        var (removalResult, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        removalResult.Should().BeNull();
        isGameOver.Should().BeTrue();
    }

    [Fact]
    public void GetNextRemoval_removes_random_piece_and_returns_legal_moves()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("e3"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));

        ChessBoard expectedBoard = new(board);
        expectedBoard.RemovePiece(new("a2"));

        var legalMoves = new LegalMoveSetFaker().Generate();
        CompressedMoves encoded = "encoded";

        _playableMoveProviderMock.CalculateAllPlayableMoves(expectedBoard).Returns(legalMoves);
        _moveEncoderMock.EncodeMoves(legalMoves.MovePaths).Returns(encoded);
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);
        _state.PlayerOvertime[GameColor.White].Remainder = TimeSpan.FromSeconds(5);
        _state.PlayerOvertime[GameColor.White].PickedNextRemoval = null;
        _state.PlayerOvertime[GameColor.Black] = CreatePlayerOvertime(
            remainder: TimeSpan.FromSeconds(6)
        );

        MockRandom(board, new("a2"), new("a1"));

        var (removalResult, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        removalResult
            .Should()
            .BeEquivalentTo(
                new OvertimeRemovalResult(
                    RemoveFrom: new("a2"),
                    NextRemoval: new("a1"),
                    NewLegalMoves: legalMoves,
                    EncodedLegalMoves: encoded
                )
            );
        isGameOver.Should().BeFalse();
        _state.PlayerOvertime[GameColor.White].Remainder.Should().Be(TimeSpan.Zero);
        _state.PlayerOvertime[GameColor.Black].Remainder.Should().Be(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public void GetNextRemoval_uses_existing_picked_piece_if_still_on_board()
    {
        ChessBoard board = new();
        AlgebraicPoint pickedPoint = new("a1");
        AlgebraicPoint nextPoint = new("e3");
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("e3"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));
        _state.PlayerOvertime[GameColor.White] = CreatePlayerOvertime(
            pickedNextRemoval: new(
                RemoveFrom: pickedPoint,
                PieceType: PieceType.Rook,
                PieceColor: GameColor.White
            )
        );

        MockRandom(board, nextPoint);
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        var (removalResult, _) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        removalResult.Should().NotBeNull();
        removalResult.RemoveFrom.Should().Be(pickedPoint);
        removalResult.NextRemoval.Should().Be(nextPoint);
    }

    [Fact]
    public void GetNextRemoval_does_not_end_game_if_a_king_is_removed_but_other_kings_remain()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));

        MockRandom(board, new AlgebraicPoint("a1"));
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        var (_, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        isGameOver.Should().BeFalse();
    }

    [Fact]
    public void GetNextRemoval_returns_game_over_when_last_king_of_color_is_removed()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));

        MockRandom(board, new AlgebraicPoint("e1"));
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        var (_, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        isGameOver.Should().BeTrue();
    }

    [Fact]
    public void GetNextRemoval_passes_only_player_pieces_to_random_provider()
    {
        ChessBoard board = new();
        var rook = PieceFactory.White(PieceType.Rook);
        var queen = PieceFactory.White(PieceType.Queen);
        var king = PieceFactory.White(PieceType.King);
        var blackKing = PieceFactory.Black(PieceType.King);
        board.PlacePiece(new("a1"), rook);
        board.PlacePiece(new("a2"), queen);
        board.PlacePiece(new("e3"), king);
        board.PlacePiece(new("e8"), blackKing);

        MockRandom(board, new AlgebraicPoint("a2"));
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        _overtime.GetNextRemoval(GameColor.White, board, _state);

        _randomMock
            .Received(1)
            .NextItemWeighted(
                ArgEx.FluentAssert<IEnumerable<(AlgebraicPoint Position, Piece Occupant)>>(x =>
                    x.Should()
                        .BeEquivalentTo(
                            [
                                (new AlgebraicPoint("a1"), rook),
                                (new AlgebraicPoint("a2"), queen),
                                (new AlgebraicPoint("e3"), king),
                            ]
                        )
                ),
                Arg.Any<Func<(AlgebraicPoint Position, Piece Occupant), int>>()
            );
    }

    [Theory]
    [InlineData(PieceType.Pawn, 4)]
    [InlineData(PieceType.UnderagePawn, 4)]
    [InlineData(PieceType.SterilePawn, 4)]
    [InlineData(PieceType.Rook, 3)]
    [InlineData(PieceType.Antiqueen, 3)]
    [InlineData(PieceType.Queen, 2)]
    [InlineData(PieceType.King, 1)]
    public void GetNextRemoval_uses_correct_weights(PieceType pieceType, int expectedWeight)
    {
        ChessBoard board = new();
        var piece = PieceFactory.White(pieceType);
        board.PlacePiece(new("a1"), piece);

        Func<(AlgebraicPoint Position, Piece Occupant), int>? capturedWeightFunc = null;
        _randomMock
            .NextItemWeighted(
                Arg.Any<IEnumerable<(AlgebraicPoint Position, Piece Occupant)>>(),
                Arg.Do<Func<(AlgebraicPoint Position, Piece Occupant), int>>(weightFunc =>
                    capturedWeightFunc = weightFunc
                )
            )
            .Returns((new AlgebraicPoint("a2"), piece));
        _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        _overtime.GetNextRemoval(GameColor.White, board, _state);

        capturedWeightFunc.Should().NotBeNull();
        capturedWeightFunc((new AlgebraicPoint("a1"), piece)).Should().Be(expectedWeight);
    }

    [Fact]
    public void StartOvertimeTurn_sets_last_move_timestamp_and_marks_player()
    {
        _overtime.StartOvertimeTurn(GameColor.White, new ChessBoard(), _state);

        _state.PlayerOvertime.Should().ContainKey(GameColor.White);
        _state.LastMoveAtTimestamp.Should().Be(_fakeNow.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void StartOvertimeTurn_picks_next_removal_if_none_picked()
    {
        ChessBoard board = new();
        AlgebraicPoint point = new("a1");
        board.PlacePiece(point, PieceFactory.White(PieceType.Rook));
        MockRandom(board, point);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(point);
        var playerOvertime = _state.PlayerOvertime[GameColor.White];

        playerOvertime
            .PickedNextRemoval.Should()
            .Be(
                new NextOvertimeRemoval(
                    RemoveFrom: point,
                    PieceType: PieceType.Rook,
                    PieceColor: GameColor.White
                )
            );
        playerOvertime.RemovalInterval.Should().Be(_settings.OvertimeInitialRemovalInterval);
        playerOvertime.Remainder.Should().Be(TimeSpan.Zero);
        board.IsEmpty(point).Should().BeFalse();
    }

    [Fact]
    public void StartOvertimeTurn_keeps_existing_picked_piece_if_still_on_board()
    {
        ChessBoard board = new();
        AlgebraicPoint pickedPoint = new("a1");
        AlgebraicPoint newPoint = new("a2");
        board.PlacePiece(pickedPoint, PieceFactory.White(PieceType.Rook));
        board.PlacePiece(newPoint, PieceFactory.White(PieceType.Queen));
        var remainder = TimeSpan.FromSeconds(2);
        _state.PlayerOvertime[GameColor.White] = CreatePlayerOvertime(
            remainder: remainder,
            pickedNextRemoval: new NextOvertimeRemoval(pickedPoint, PieceType.Rook, GameColor.White)
        );
        MockRandom(board, newPoint);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(pickedPoint);
        var playerOvertime = _state.PlayerOvertime[GameColor.White];
        playerOvertime
            .PickedNextRemoval.Should()
            .Be(new NextOvertimeRemoval(pickedPoint, PieceType.Rook, GameColor.White));
        playerOvertime.RemovalInterval.Should().Be(_settings.OvertimeInitialRemovalInterval);
        playerOvertime.Remainder.Should().Be(remainder);
    }

    [Fact]
    public void StartOvertimeTurn_picks_new_piece_if_existing_not_on_board()
    {
        ChessBoard board = new();
        AlgebraicPoint point = new("a2");
        board.PlacePiece(point, PieceFactory.White(PieceType.Rook));
        TimeSpan removalInterval =
            _settings.OvertimeSaveIntervalReduction + TimeSpan.FromSeconds(5);
        _state.PlayerOvertime[GameColor.White] = CreatePlayerOvertime(
            remainder: TimeSpan.FromSeconds(3),
            removalInterval: removalInterval,
            pickedNextRemoval: new NextOvertimeRemoval(
                new AlgebraicPoint("a1"),
                PieceType.Knook,
                GameColor.White
            )
        );
        MockRandom(board, point);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(point);
        var playerOvertime = _state.PlayerOvertime[GameColor.White];
        playerOvertime.PickedNextRemoval.Should().NotBeNull();
        playerOvertime.PickedNextRemoval.RemoveFrom.Should().Be(point);
        playerOvertime.Remainder.Should().Be(TimeSpan.Zero);
        playerOvertime
            .RemovalInterval.Should()
            .Be(removalInterval - _settings.OvertimeSaveIntervalReduction);
    }

    [Fact]
    public void StartOvertimeTurn_picks_new_piece_if_existing_piece_has_wrong_color_even_if_type_matches()
    {
        ChessBoard board = new();
        AlgebraicPoint stalePoint = new("a1");
        AlgebraicPoint newPoint = new("b2");
        board.PlacePiece(stalePoint, PieceFactory.Black(PieceType.Rook));
        board.PlacePiece(newPoint, PieceFactory.White(PieceType.Queen));
        _state.PlayerOvertime[GameColor.White] = CreatePlayerOvertime(
            pickedNextRemoval: new NextOvertimeRemoval(stalePoint, PieceType.Rook, GameColor.White)
        );

        MockRandom(board, newPoint);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(newPoint);
        var nextRemoval = _state.PlayerOvertime[GameColor.White].PickedNextRemoval;
        nextRemoval.Should().NotBeNull();
        nextRemoval.RemoveFrom.Should().Be(newPoint);
    }

    [Fact]
    public void StartOvertimeTurn_picks_new_piece_if_existing_piece_has_wrong_type_even_if_color_matches()
    {
        ChessBoard board = new();
        AlgebraicPoint stalePoint = new("a1");
        AlgebraicPoint newPoint = new("b2");
        board.PlacePiece(stalePoint, PieceFactory.White(PieceType.Queen));
        board.PlacePiece(newPoint, PieceFactory.White(PieceType.Rook));
        _state.PlayerOvertime[GameColor.White] = CreatePlayerOvertime(
            pickedNextRemoval: new NextOvertimeRemoval(stalePoint, PieceType.Rook, GameColor.White)
        );

        MockRandom(board, newPoint);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(newPoint);
        var nextRemoval = _state.PlayerOvertime[GameColor.White].PickedNextRemoval;
        nextRemoval.Should().NotBeNull();
        nextRemoval.RemoveFrom.Should().Be(newPoint);
    }

    [Fact]
    public void TryEndOvertimeTurn_does_nothing_if_player_never_entered_overtime()
    {
        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        _state.PlayerOvertime.Should().BeEmpty();
    }

    [Fact]
    public void TryEndOvertimeTurn_sets_remainder_for_player_using_player_removal_interval()
    {
        _overtime.StartOvertimeTurn(GameColor.White, new ChessBoard(), _state);

        var removalInterval = TimeSpan.FromMilliseconds(2000);
        _state.PlayerOvertime[GameColor.White].RemovalInterval = removalInterval;

        // advance time by 5 intervals + 750ms
        double addMs = removalInterval.TotalMilliseconds * 5 + 750;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(addMs));

        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        var expected = TimeSpan.FromMilliseconds(addMs % removalInterval.TotalMilliseconds);

        _state.PlayerOvertime[GameColor.White].Remainder.Should().Be(expected);
        _state.PlayerOvertime.Should().NotContainKey(GameColor.Black);
    }

    [Fact]
    public void TryEndOvertimeTurn_increments_remainder_using_player_removal_interval()
    {
        _overtime.StartOvertimeTurn(GameColor.White, new ChessBoard(), _state);

        var removalInterval = TimeSpan.FromMilliseconds(1200);
        _state.PlayerOvertime[GameColor.White].RemovalInterval = removalInterval;
        _state.PlayerOvertime[GameColor.White].Remainder = TimeSpan.FromMilliseconds(500);

        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(750));

        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        var expected = TimeSpan.FromMilliseconds(500 + 750 % removalInterval.TotalMilliseconds);
        _state.PlayerOvertime[GameColor.White].Remainder.Should().Be(expected);
    }

    [Fact]
    public void GetTimeUntilNextRemoval_returns_full_interval_if_no_player_overtime()
    {
        _state.PlayerOvertime[GameColor.Black] = CreatePlayerOvertime(
            removalInterval: _settings.OvertimeInitialRemovalInterval + TimeSpan.FromSeconds(1)
        );

        var result = _overtime.GetTimeUntilNextRemoval(GameColor.White, _state);

        result.Should().Be(_settings.OvertimeInitialRemovalInterval);
    }

    [Fact]
    public void GetTimeUntilNextRemoval_accounts_for_remainder()
    {
        var removalInterval = TimeSpan.FromMilliseconds(1200);
        var remainder = TimeSpan.FromMilliseconds(400);
        _state.PlayerOvertime[GameColor.White] = CreatePlayerOvertime(
            remainder: remainder,
            removalInterval: removalInterval
        );
        _state.PlayerOvertime[GameColor.Black] = CreatePlayerOvertime(
            remainder: TimeSpan.FromMilliseconds(100)
        );

        var result = _overtime.GetTimeUntilNextRemoval(GameColor.White, _state);

        result.Should().Be(removalInterval - TimeSpan.FromMilliseconds(400));
    }

    [Fact]
    public void HasEnteredOvertime_returns_false_if_player_not_present()
    {
        _state.PlayerOvertime[GameColor.Black] = CreatePlayerOvertime();

        bool result = _overtime.HasEnteredOvertime(GameColor.White, _state);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasEnteredOvertime_returns_true_if_player_started_overtime()
    {
        _state.PlayerOvertime[GameColor.Black] = CreatePlayerOvertime();

        bool result = _overtime.HasEnteredOvertime(GameColor.Black, _state);

        result.Should().BeTrue();
    }
}
