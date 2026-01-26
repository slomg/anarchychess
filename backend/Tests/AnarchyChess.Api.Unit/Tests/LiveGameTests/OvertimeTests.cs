using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
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
    public void GetNextRemoval_returns_game_over_if_player_has_no_pieces()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));
        _overtime.StartOvertimeTurn(GameColor.White, _state);

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

        _randomMock.Next(3).Returns(1); // pick queen

        ChessBoard expectedBoard = new(board);
        expectedBoard.RemovePiece(new("a2"));

        var legalMoves = new LegalMoveSetFaker().Generate();
        CompressedMoves encoded = "encoded";

        _playableMoveProviderMock.CalculateAllPlayableMoves(expectedBoard).Returns(legalMoves);
        _moveEncoderMock.EncodeMoves(legalMoves.MovePaths).Returns(encoded);
        _overtime.StartOvertimeTurn(GameColor.White, _state);
        _state.PlayerRemainder[GameColor.White] = TimeSpan.FromSeconds(5);
        _state.PlayerRemainder[GameColor.Black] = TimeSpan.FromSeconds(6);

        var (removalResult, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        removalResult
            .Should()
            .BeEquivalentTo(
                new OvertimeRemovalResult(
                    RemoveFrom: new("a2"),
                    NewLegalMoves: legalMoves,
                    EncodedLegalMoves: encoded
                )
            );
        isGameOver.Should().BeFalse();
        _state.PlayerRemainder[GameColor.White].Should().Be(TimeSpan.Zero);
        _state.PlayerRemainder[GameColor.Black].Should().Be(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public void GetNextRemoval_does_not_end_game_if_a_king_is_removed_but_other_kings_remain()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));

        _randomMock.Next(2).Returns(0); // king on a1

        var legalMoves = new LegalMoveSetFaker().Generate();
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(Arg.Any<IReadOnlyChessBoard>())
            .Returns(legalMoves);
        _overtime.StartOvertimeTurn(GameColor.White, _state);

        var (_, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        isGameOver.Should().BeFalse();
    }

    [Fact]
    public void GetNextRemoval_returns_game_over_when_last_king_of_color_is_removed()
    {
        ChessBoard board = new();
        board.PlacePiece(new("e1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("e8"), PieceFactory.Black(PieceType.King));

        _randomMock.Next(1).Returns(0);

        var legalMoves = new LegalMoveSetFaker().Generate();
        _playableMoveProviderMock
            .CalculateAllPlayableMoves(Arg.Any<IReadOnlyChessBoard>())
            .Returns(legalMoves);
        _overtime.StartOvertimeTurn(GameColor.White, _state);

        var (_, isGameOver) = _overtime.GetNextRemoval(GameColor.White, board, _state);

        isGameOver.Should().BeTrue();
    }

    [Fact]
    public void StartOvertimeTurn_sets_last_move_timestamp_and_marks_player()
    {
        _overtime.StartOvertimeTurn(GameColor.White, _state);

        _state.PlayersEnteredOvertime.Should().Contain(GameColor.White);
        _state.LastMoveAtTimestamp.Should().Be(_fakeNow.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void TryEndOvertimeTurn_does_nothing_if_player_never_entered_overtime()
    {
        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        _state.PlayerRemainder.Should().BeEmpty();
    }

    [Fact]
    public void TryEndOvertimeTurn_sets_remainder_for_player()
    {
        _overtime.StartOvertimeTurn(GameColor.White, _state);

        double addMs = _settings.OvertimeRemovalInterval.TotalMilliseconds * 5 + 750;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow.AddMilliseconds(addMs));

        _overtime.TryEndOvertimeTurn(GameColor.White, _state);

        var expected = TimeSpan.FromMilliseconds(
            addMs % _settings.OvertimeRemovalInterval.TotalMilliseconds
        );

        _state.PlayerRemainder[GameColor.White].Should().Be(expected);
        _state.PlayerRemainder.Should().NotContainKey(GameColor.Black);
    }

    [Fact]
    public void GetTimeUntilNextRemoval_returns_full_interval_if_no_remainder()
    {
        _state.PlayerRemainder[GameColor.Black] = TimeSpan.FromMilliseconds(4);

        var result = _overtime.GetTimeUntilNextRemoval(GameColor.White, _state);

        result.Should().Be(_settings.OvertimeRemovalInterval);
    }

    [Fact]
    public void GetTimeUntilNextRemoval_accounts_for_remainder()
    {
        _state.PlayerRemainder[GameColor.White] = TimeSpan.FromMilliseconds(400);
        _state.PlayerRemainder[GameColor.Black] = TimeSpan.FromMilliseconds(100);

        var result = _overtime.GetTimeUntilNextRemoval(GameColor.White, _state);

        result.Should().Be(_settings.OvertimeRemovalInterval - TimeSpan.FromMilliseconds(400));
    }

    [Fact]
    public void HasEnteredOvertime_returns_false_if_player_not_present()
    {
        _state.PlayersEnteredOvertime.Add(GameColor.Black);

        bool result = _overtime.HasEnteredOvertime(GameColor.White, _state);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasEnteredOvertime_returns_true_if_player_started_overtime()
    {
        _state.PlayersEnteredOvertime.Add(GameColor.Black);

        bool result = _overtime.HasEnteredOvertime(GameColor.Black, _state);

        result.Should().BeTrue();
    }
}
