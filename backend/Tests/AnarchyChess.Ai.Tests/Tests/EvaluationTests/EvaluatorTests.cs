using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class EvaluatorTests
{
    private readonly Evaluator _evaluator = new();

    [Fact]
    public void Evaluate_returns_0_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);

        int score = _evaluator.Evaluate(board);

        score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_uses_white_as_our_color_when_white_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
        };

        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: true);

        int score = _evaluator.Evaluate(board);

        score.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Evaluate_uses_black_as_our_color_when_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
        };

        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        int score = _evaluator.Evaluate(board);

        score.Should().BeLessThan(0);
    }
}
