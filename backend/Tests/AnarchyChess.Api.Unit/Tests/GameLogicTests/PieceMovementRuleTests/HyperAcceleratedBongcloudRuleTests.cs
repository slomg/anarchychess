using AnarchyChess.Api.Game;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class HyperAcceleratedBongcloudRuleTests
{
    private readonly HyperAcceleratedBongcloudRule _rule = new();

    [Fact]
    public void Evaluate_creates_the_correct_first_move_for_white()
    {
        ChessBoard board = new(GameConstants.StartingPosition);
        Piece piece = new(PieceType.King, GameColor.White, HasMoved: false);

        _rule
            .Evaluate(board, new("f1"), piece)
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new Move(
                    from: new("f1"),
                    to: new("f2"),
                    piece,
                    captures: [new MoveCapture(new("f2"), board)],
                    specialMoveType: SpecialMoveType.HyperAcceleratedBongcloud
                )
            );
    }

    [Fact]
    public void Evaluate_creates_the_correct_first_move_for_black()
    {
        ChessBoard board = new(GameConstants.StartingPosition);
        Piece piece = new(PieceType.King, GameColor.Black, HasMoved: false);

        _rule
            .Evaluate(board, new("f10"), piece)
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new Move(
                    from: new("f10"),
                    to: new("f9"),
                    piece,
                    captures: [new MoveCapture(new("f9"), board)],
                    specialMoveType: SpecialMoveType.HyperAcceleratedBongcloud
                )
            );
    }

    [Fact]
    public void Evaluate_only_allows_captures()
    {
        Piece piece = new(PieceType.King, GameColor.White, HasMoved: false);
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f1")] = piece,
                [new("f3")] = PieceFactory.White(),
            }
        );

        _rule.Evaluate(board, new("f1"), piece).Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_only_allows_self_captures()
    {
        Piece piece = new(PieceType.King, GameColor.White, HasMoved: false);
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f1")] = piece,
                [new("f2")] = PieceFactory.Black(),
            }
        );

        _rule.Evaluate(board, new("f1"), piece).Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_no_move_after_the_first_move_was_made()
    {
        ChessBoard board = new(GameConstants.StartingPosition, moves: new MoveFaker().Generate(2));
        Piece piece = new(PieceType.King, GameColor.White, HasMoved: false);

        _rule.Evaluate(board, new("f1"), piece).Should().BeEmpty();
    }
}
