using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class DrawEvaluatorTests
{
    private readonly GameResultDescriber _describer = new();
    private readonly DrawEvaulator _drawEvaluator;
    private readonly AutoDrawState _state;
    private readonly ChessBoard _board = new();

    public DrawEvaluatorTests()
    {
        _drawEvaluator = new DrawEvaulator(_describer);
        _state = new AutoDrawState();
    }

    [Fact]
    public void TryEvaluateDraw_returns_false_with_no_draw_condition()
    {
        Move move = new(from: new("a1"), to: new("a2"), piece: PieceFactory.White());

        var result = _drawEvaluator.TryEvaluateDraw(
            move,
            new FenNotationFaker().Generate(),
            _board,
            _state,
            out var endStatus
        );

        result.Should().BeFalse();
        endStatus.Should().BeNull();
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_after_three_fold_repetition()
    {
        Move move = new(from: new("a1"), to: new("a2"), piece: PieceFactory.White());
        var fen = new FenNotationFaker().Generate();

        _drawEvaluator.TryEvaluateDraw(move, fen, _board, _state, out _).Should().BeFalse();
        _drawEvaluator.TryEvaluateDraw(move, fen, _board, _state, out _).Should().BeFalse();
        var result = _drawEvaluator.TryEvaluateDraw(move, fen, _board, _state, out var endStatus);

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.ThreeFold());
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_when_HalfMoveClock_is_100()
    {
        Move move = new(
            from: new("a1"),
            to: new("a2"),
            piece: PieceFactory.White(PieceType.Horsey)
        );

        var result = _drawEvaluator.TryEvaluateDraw(
            move,
            new FenNotationFaker().Generate(),
            new ChessBoard(halfMoveClock: 100),
            _state,
            out var endStatus
        );

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.FiftyMoves());
    }

    [Fact]
    public void TryEvaluateDraw_returns_true_on_enemy_king_touch()
    {
        Move move = new(from: new("a1"), to: new("a2"), piece: PieceFactory.White(PieceType.King));
        _board.PlacePiece(new AlgebraicPoint("a3"), PieceFactory.Black(PieceType.King));

        var result = _drawEvaluator.TryEvaluateDraw(
            move,
            new FenNotationFaker().Generate(),
            _board,
            _state,
            out var endStatus
        );

        result.Should().BeTrue();
        endStatus.Should().BeEquivalentTo(_describer.KingTouch());
    }

    [Fact]
    public void TryEvaluateDraw_returns_false_on_friendly_king_touch()
    {
        Move move = new(from: new("a1"), to: new("a2"), piece: PieceFactory.White(PieceType.King));
        _board.PlacePiece(new AlgebraicPoint("a3"), PieceFactory.White(PieceType.King));

        var result = _drawEvaluator.TryEvaluateDraw(
            move,
            new FenNotationFaker().Generate(),
            _board,
            _state,
            out var endStatus
        );

        result.Should().BeFalse();
        endStatus.Should().BeNull();
    }
}
