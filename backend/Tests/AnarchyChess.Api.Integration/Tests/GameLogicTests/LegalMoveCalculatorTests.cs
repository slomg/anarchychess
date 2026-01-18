using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests;

public class LegalMoveCalculatorTests : BaseIntegrationTest
{
    private readonly ILegalMoveCalculator _calculator;

    public LegalMoveCalculatorTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _calculator = Scope.ServiceProvider.GetRequiredService<ILegalMoveCalculator>();
    }

    [Fact]
    public void Constructor_throws_if_not_all_piece_types_are_defined()
    {
        var act = () => new LegalMoveCalculator([], []);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Could not find definitions for all pieces");
    }

    [Fact]
    public void CalculateAllLegalMoves_includes_moves_from_forever_rules()
    {
        Move lastMove = new(
            from: new AlgebraicPoint("g2"),
            to: new AlgebraicPoint("h3"),
            piece: PieceFactory.Black(), // color mismatch so no other move
            captures: [new MoveCapture(PieceFactory.White(), new AlgebraicPoint("h3"))]
        );
        ChessBoard board = new(moves: [lastMove], sideToMove: GameColor.White);
        board.PlacePiece(lastMove.To, lastMove.Piece);

        var moves = _calculator.CalculateAllLegalMoves(board).ToList();

        moves
            .Should()
            .ContainSingle(move => move.SpecialMoveType == SpecialMoveType.OmnipotentPawnSpawn);
    }

    [Fact]
    public void CalculateAllLegalMoves_only_returns_the_moves_for_the_right_color()
    {
        ChessBoard board = new(sideToMove: GameColor.White);
        board.PlacePiece(new AlgebraicPoint("a1"), PieceFactory.White(PieceType.Pawn));
        board.PlacePiece(new AlgebraicPoint("a3"), PieceFactory.Black(PieceType.King));

        var moves = _calculator.CalculateAllLegalMoves(board);

        moves
            .Should()
            .ContainSingle()
            .Which.Should()
            .Satisfy<Move>(move =>
            {
                move.From.Should().BeEquivalentTo(new AlgebraicPoint("a1"));
                move.To.Should().BeEquivalentTo(new AlgebraicPoint("a2"));
            });
    }

    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    public void CalculateLegalMoves_allows_moves_for_piece_with_neutral_color(GameColor sideToMove)
    {
        ChessBoard board = new(sideToMove: sideToMove);
        var neutralPiece = PieceFactory.Neutral(PieceType.TraitorRook);
        board.PlacePiece(new AlgebraicPoint("d4"), neutralPiece);

        // surround it with equal white and black pieces to trigger neutral behavior
        board.PlacePiece(new AlgebraicPoint("c3"), PieceFactory.White(PieceType.Pawn));
        board.PlacePiece(new AlgebraicPoint("e5"), PieceFactory.Black(PieceType.Pawn));

        var moves = _calculator.CalculateAllLegalMoves(board).ToList();

        moves.Should().Contain(move => move.Piece.Type == PieceType.TraitorRook);
    }

    [Fact]
    public void CalculateForeverRules_returns_only_forever_moves()
    {
        Move lastMove = new(
            from: new AlgebraicPoint("g2"),
            to: new AlgebraicPoint("h3"),
            piece: PieceFactory.White(),
            captures: [new MoveCapture(PieceFactory.White(), new AlgebraicPoint("h3"))]
        );
        ChessBoard board = new(moves: [lastMove], sideToMove: GameColor.White);
        board.PlacePiece(lastMove.To, lastMove.Piece);

        var moves = _calculator.CalculateForeverRules(board).ToList();

        moves
            .Should()
            .ContainSingle(move => move.SpecialMoveType == SpecialMoveType.OmnipotentPawnSpawn);
    }
}
