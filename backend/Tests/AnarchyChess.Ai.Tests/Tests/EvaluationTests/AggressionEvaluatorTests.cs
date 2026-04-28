using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class AggressionEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = new();

        EvaluationResult evaluation = AggressionEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
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

        EvaluationResult evaluation = AggressionEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
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

        EvaluationResult evaluation = AggressionEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 3);
        evaluation.BlackScore.Should().Be(0);
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

        EvaluationResult evaluation = AggressionEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 3);
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

        EvaluationResult evaluation = AggressionEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 2);
        evaluation.BlackScore.Should().Be(AggressionEvaluator.MaxDistanceBonus - 2);
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

        EvaluationResult evaluation = AggressionEvaluator.Evaluate(board);

        evaluation
            .WhiteScore.Should()
            .Be(
                (AggressionEvaluator.MaxDistanceBonus - 1)
                    + (AggressionEvaluator.MaxDistanceBonus - 1)
            );
        evaluation
            .BlackScore.Should()
            .Be(
                (AggressionEvaluator.MaxDistanceBonus - 1)
                    + (AggressionEvaluator.MaxDistanceBonus - 2)
            );
    }
}
