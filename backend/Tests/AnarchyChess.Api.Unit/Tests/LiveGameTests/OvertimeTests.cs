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
        _state.PlayerOvertime[GameColor.Black] = new PlayerOvertime
        {
            Remainder = TimeSpan.FromSeconds(6),
        };
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
        _state.PlayerOvertime[GameColor.White] = new PlayerOvertime
        {
            PickedNextRemoval = new(
                RemoveFrom: pickedPoint,
                PieceType: PieceType.Rook,
                PieceColor: GameColor.White
            ),
        };
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
        _state
            .PlayerOvertime[GameColor.White]
            .PickedNextRemoval.Should()
            .Be(
                new NextOvertimeRemoval(
                    RemoveFrom: point,
                    PieceType: PieceType.Rook,
                    PieceColor: GameColor.White
                )
            );
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
        _state.PlayerOvertime[GameColor.White] = new PlayerOvertime
        {
            PickedNextRemoval = new(
                RemoveFrom: pickedPoint,
                PieceType: PieceType.Rook,
                PieceColor: GameColor.White
            ),
        };
        MockRandom(board, newPoint);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(pickedPoint);
        _state
            .PlayerOvertime[GameColor.White]
            .PickedNextRemoval.Should()
            .Be(
                new NextOvertimeRemoval(
                    RemoveFrom: pickedPoint,
                    PieceType: PieceType.Rook,
                    PieceColor: GameColor.White
                )
            );
    }

    [Fact]
    public void StartOvertimeTurn_picks_new_piece_if_existing_not_on_board()
    {
        ChessBoard board = new();
        AlgebraicPoint point = new("a2");
        board.PlacePiece(point, PieceFactory.White(PieceType.Rook));
        _state.PlayerOvertime[GameColor.White] = new PlayerOvertime
        {
            PickedNextRemoval = new(
                RemoveFrom: new AlgebraicPoint("a1"),
                PieceType: PieceType.Knook,
                GameColor.White
            ),
        };
        MockRandom(board, point);

        var result = _overtime.StartOvertimeTurn(GameColor.White, board, _state);

        result.Should().Be(point);
        var nextRemoval = _state.PlayerOvertime[GameColor.White].PickedNextRemoval;
        nextRemoval.Should().NotBeNull();
        nextRemoval.RemoveFrom.Should().Be(point);
    }

    [Fact]
    public void StartOvertimeTurn_picks_new_piece_if_existing_piece_has_wrong_color_even_if_type_matches()
    {
        ChessBoard board = new();
        AlgebraicPoint stalePoint = new("a1");
        AlgebraicPoint newPoint = new("b2");
        board.PlacePiece(stalePoint, PieceFactory.Black(PieceType.Rook));
        board.PlacePiece(newPoint, PieceFactory.White(PieceType.Queen));
        _state.PlayerOvertime[GameColor.White] = new PlayerOvertime
        {
            PickedNextRemoval = new(
                RemoveFrom: stalePoint,
                PieceType: PieceType.Rook,
                PieceColor: GameColor.White
            ),
        };

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
        _state.PlayerOvertime[GameColor.White] = new PlayerOvertime
        {
            PickedNextRemoval = new(
                RemoveFrom: stalePoint,
                PieceType: PieceType.Rook,
                PieceColor: GameColor.White
            ),
        };

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
    public void TryEndOvertimeTurn_sets_remainder_for_player()
    {
        _overtime.StartOvertimeTurn(GameColor.White, new ChessBoard(), _state);

        double addMs = _settings.OvertimeRemovalInterval.TotalMilliseconds * 5 + 750;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(addMs));

        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        var expected = TimeSpan.FromMilliseconds(
            addMs % _settings.OvertimeRemovalInterval.TotalMilliseconds
        );

        _state.PlayerOvertime[GameColor.White].Remainder.Should().Be(expected);
        _state.PlayerOvertime.Should().NotContainKey(GameColor.Black);
    }

    [Fact]
    public void TryEndOvertimeTurn_increments_remainder()
    {
        _overtime.StartOvertimeTurn(GameColor.White, new ChessBoard(), _state);
        _state.PlayerOvertime[GameColor.White].Remainder = TimeSpan.FromMilliseconds(500);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(750));

        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        var expected = TimeSpan.FromMilliseconds(
            500 + 750 % _settings.OvertimeRemovalInterval.TotalMilliseconds
        );
        _state.PlayerOvertime[GameColor.White].Remainder.Should().Be(expected);
    }

    [Fact]
    public void TryEndOvertimeTurn_does_not_overflow_remainder()
    {
        _overtime.StartOvertimeTurn(GameColor.White, new ChessBoard(), _state);
        _state.PlayerOvertime[GameColor.White].Remainder =
            _settings.OvertimeRemovalInterval - TimeSpan.FromMilliseconds(200);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(500));

        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        _state
            .PlayerOvertime[GameColor.White]
            .Remainder.Should()
            .Be(_settings.OvertimeRemovalInterval);
    }

    [Fact]
    public void GetTimeUntilNextRemoval_returns_full_interval_if_no_remainder()
    {
        _state.PlayerOvertime[GameColor.Black] = new PlayerOvertime
        {
            Remainder = TimeSpan.FromMilliseconds(4),
        };

        var result = _overtime.GetTimeUntilNextRemoval(GameColor.White, _state);

        result.Should().Be(_settings.OvertimeRemovalInterval);
    }

    [Fact]
    public void GetTimeUntilNextRemoval_accounts_for_remainder()
    {
        _state.PlayerOvertime[GameColor.White] = new PlayerOvertime
        {
            Remainder = TimeSpan.FromMilliseconds(400),
        };
        _state.PlayerOvertime[GameColor.Black] = new PlayerOvertime
        {
            Remainder = TimeSpan.FromMilliseconds(100),
        };

        var result = _overtime.GetTimeUntilNextRemoval(GameColor.White, _state);

        result.Should().Be(_settings.OvertimeRemovalInterval - TimeSpan.FromMilliseconds(400));
    }

    [Fact]
    public void HasEnteredOvertime_returns_false_if_player_not_present()
    {
        _state.PlayerOvertime[GameColor.Black] = new PlayerOvertime();

        bool result = _overtime.HasEnteredOvertime(GameColor.White, _state);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasEnteredOvertime_returns_true_if_player_started_overtime()
    {
        _state.PlayerOvertime[GameColor.Black] = new PlayerOvertime();

        bool result = _overtime.HasEnteredOvertime(GameColor.Black, _state);

        result.Should().BeTrue();
    }
}
