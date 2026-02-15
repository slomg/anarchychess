using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class AggressionEvaluatorTests
{
    private readonly AggressionEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_zero_when_only_kings_present()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_single_piece_aggression()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("b5")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 3);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_evaluates_black()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("b4")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 3);
    }

    [Fact]
    public void Evaluate_evaluates_both_colors()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("c6")] = PieceFactory.White(PieceType.Rook),
            [new("c3")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 2);
        blackScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 2);
    }

    [Fact]
    public void Evaluate_handles_multiple_pieces_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),

            [new("d7")] = PieceFactory.White(PieceType.Rook),
            [new("f7")] = PieceFactory.White(PieceType.Bishop),
            [new("c3")] = PieceFactory.Black(PieceType.Rook),
            [new("f2")] = PieceFactory.Black(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        // White pieces distances to black king:
        // d7 -> e8 = 1
        // f7 -> e8 = 1
        // Black pieces distances to white king:
        // f2 -> e1 = 1
        // c3 -> e1 = 2
        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore
            .Should()
            .Be(
                (AggressionEvaluator.MaxDistanceBonus - 1)
                    + (AggressionEvaluator.MaxDistanceBonus - 1)
            );
        blackScore
            .Should()
            .Be(
                (AggressionEvaluator.MaxDistanceBonus - 1)
                    + (AggressionEvaluator.MaxDistanceBonus - 2)
            );
    }
}
