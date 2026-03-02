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

    [Fact]
    public void TryEvaluateTermination_returns_false_when_kings_present_and_not_adjacent()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.King),
            [new("h8")] = PieceFactory.Black(PieceType.King),
        };

        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: true);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 12, out int terminationEval);

        result.Should().BeFalse();
        terminationEval.Should().Be(0);
    }

    [Fact]
    public void TryEvaluateTermination_returns_correct_eval_when_white_has_no_kings_and_white_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("h8")] = PieceFactory.Black(PieceType.King),
        };

        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: true);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 15, out int terminationEval);

        result.Should().BeTrue();
        terminationEval.Should().Be(-100_015);
    }

    [Fact]
    public void TryEvaluateTermination_returns_correct_eval_when_white_has_no_kings_and_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("h8")] = PieceFactory.Black(PieceType.King),
        };

        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 12, out int terminationEval);

        result.Should().BeTrue();
        terminationEval.Should().Be(100_012);
    }

    [Fact]
    public void TryEvaluateTermination_returns_correct_eval_when_black_has_no_kings_and_white_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.King),
        };

        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: true);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 6, out int terminationEval);

        result.Should().BeTrue();
        terminationEval.Should().Be(100_006);
    }

    [Fact]
    public void TryEvaluateTermination_returns_correct_eval_when_black_has_no_kings_and_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 3, out int terminationEval);

        result.Should().BeTrue();
        terminationEval.Should().Be(-100_003);
    }

    [Fact]
    public void TryEvaluateTermination_returns_draw_when_both_sides_have_no_kings()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("c1")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 21, out int terminationEval);

        result.Should().BeTrue();
        terminationEval.Should().Be(0);
    }

    [Fact]
    public void TryEvaluateTermination_returns_draw_when_kings_are_adjacent()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e4")] = PieceFactory.White(PieceType.King),
            [new("e5")] = PieceFactory.Black(PieceType.King),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        bool result = _evaluator.TryEvaluateTermination(board, depth: 69, out int terminationEval);

        result.Should().BeTrue();
        terminationEval.Should().Be(0);
    }
}
