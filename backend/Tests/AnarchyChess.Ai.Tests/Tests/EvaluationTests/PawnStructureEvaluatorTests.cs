using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class PawnStructureEvaluatorTests
{
    private readonly PawnStructureEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = new();

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_zero_on_normal_structure()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("b4")] = PieceFactory.White(PieceType.Pawn),
            [new("c5")] = PieceFactory.White(PieceType.Pawn),
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
            [new("e5")] = PieceFactory.White(PieceType.Pawn),

            [new("g3")] = PieceFactory.White(PieceType.Pawn),
            [new("h3")] = PieceFactory.White(PieceType.Pawn),
            [new("i3")] = PieceFactory.White(PieceType.Pawn),
            [new("j3")] = PieceFactory.White(PieceType.Pawn),

            [new("a8")] = PieceFactory.Black(PieceType.Pawn),
            [new("b8")] = PieceFactory.Black(PieceType.Pawn),

            [new("d7")] = PieceFactory.Black(PieceType.Pawn),
            [new("e8")] = PieceFactory.Black(PieceType.Pawn),
            [new("f7")] = PieceFactory.Black(PieceType.Pawn),
            [new("g6")] = PieceFactory.Black(PieceType.Pawn),
            [new("h7")] = PieceFactory.Black(PieceType.Pawn),
            [new("i8")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_black_score()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("b4")] = PieceFactory.White(PieceType.Pawn),
            [new("c4")] = PieceFactory.White(PieceType.Pawn),

            [new("a7")] = PieceFactory.Black(PieceType.Pawn),
            [new("a8")] = PieceFactory.Black(PieceType.Pawn),
            [new("c7")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore
            .Should()
            .Be(
                -PawnStructureEvaluator.DoubledPenalty
                    - (PawnStructureEvaluator.IsolatedPenalty * 3)
            );
    }

    [Fact]
    public void Evaluate_includes_underage_pawns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d8")] = PieceFactory.Black(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.UnderagePawn),

            [new("d2")] = PieceFactory.White(PieceType.Pawn),
            [new("d3")] = PieceFactory.White(PieceType.UnderagePawn),
            [new("d4")] = PieceFactory.White(PieceType.UnderagePawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore
            .Should()
            .Be(
                -(PawnStructureEvaluator.DoubledPenalty * 2)
                    - (PawnStructureEvaluator.IsolatedPenalty * 3)
            );
        blackScore
            .Should()
            .Be(
                -PawnStructureEvaluator.DoubledPenalty
                    - (PawnStructureEvaluator.IsolatedPenalty * 2)
            );
    }

    [Fact]
    public void Evaluate_counts_doubled_pawns_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a4")] = PieceFactory.White(PieceType.Pawn),
            [new("a5")] = PieceFactory.White(PieceType.Pawn),
            [new("b5")] = PieceFactory.White(PieceType.Pawn),
            [new("a9")] = PieceFactory.Black(PieceType.Pawn),
            [new("b9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = _evaluator.Evaluate(board, endgameFactor: 0).WhiteScore;

        whiteScore.Should().Be(-PawnStructureEvaluator.DoubledPenalty);
    }

    [Fact]
    public void Evaluate_counts_isolated_pawns_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a2")] = PieceFactory.White(PieceType.Pawn),
            [new("j2")] = PieceFactory.White(PieceType.Pawn),
            [new("d2")] = PieceFactory.White(PieceType.Pawn),
            [new("g2")] = PieceFactory.White(PieceType.Pawn),
            [new("h2")] = PieceFactory.White(PieceType.Pawn),

            [new("a9")] = PieceFactory.Black(PieceType.Pawn),
            [new("b9")] = PieceFactory.Black(PieceType.Pawn),
            [new("c9")] = PieceFactory.Black(PieceType.Pawn),
            [new("d9")] = PieceFactory.Black(PieceType.Pawn),
            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f9")] = PieceFactory.Black(PieceType.Pawn),
            [new("g9")] = PieceFactory.Black(PieceType.Pawn),
            [new("h9")] = PieceFactory.Black(PieceType.Pawn),
            [new("i9")] = PieceFactory.Black(PieceType.Pawn),
            [new("j9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = _evaluator.Evaluate(board, endgameFactor: 0).WhiteScore;

        whiteScore.Should().Be(-PawnStructureEvaluator.IsolatedPenalty * 3);
    }

    [Fact]
    public void Evaluate_counts_neighboring_pawns_that_are_too_far_away_rankwise_isolated()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e4")] = PieceFactory.White(PieceType.Pawn),
            [new("f6")] = PieceFactory.White(PieceType.Pawn),

            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = _evaluator.Evaluate(board, endgameFactor: 0).WhiteScore;

        whiteScore.Should().Be(-PawnStructureEvaluator.IsolatedPenalty * 2);
    }

    [Fact]
    public void Evaluate_counts_white_backwards_pawns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("b4")] = PieceFactory.White(PieceType.Pawn),
            [new("c4")] = PieceFactory.White(PieceType.Pawn),

            [new("e4")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.White(PieceType.Pawn),

            [new("c6")] = PieceFactory.Black(PieceType.Pawn),
            [new("d6")] = PieceFactory.Black(PieceType.Pawn),
            [new("f7")] = PieceFactory.Black(PieceType.Pawn),
            [new("g7")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = _evaluator.Evaluate(board, endgameFactor: 0).WhiteScore;

        whiteScore.Should().Be(-PawnStructureEvaluator.BackwardsPenalty);
    }

    [Fact]
    public void Evaluate_counts_black_backwards_pawns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("g8")] = PieceFactory.Black(PieceType.Pawn),
            [new("h8")] = PieceFactory.Black(PieceType.Pawn),

            [new("e7")] = PieceFactory.Black(PieceType.Pawn),
            [new("d6")] = PieceFactory.Black(PieceType.Pawn),

            [new("c4")] = PieceFactory.White(PieceType.Pawn),
            [new("d4")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.White(PieceType.Pawn),
            [new("g5")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        int blackScore = _evaluator.Evaluate(board, endgameFactor: 0).BlackScore;

        blackScore.Should().Be(-PawnStructureEvaluator.BackwardsPenalty);
    }

    [Fact]
    public void Evaluate_counts_white_passed_pawns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("b4")] = PieceFactory.White(PieceType.Pawn),
            [new("c5")] = PieceFactory.White(PieceType.Pawn),

            [new("e4")] = PieceFactory.White(PieceType.Pawn),
            [new("f4")] = PieceFactory.White(PieceType.Pawn),

            [new("i7")] = PieceFactory.White(PieceType.Pawn),
            [new("j7")] = PieceFactory.White(PieceType.Pawn),

            [new("e7")] = PieceFactory.Black(PieceType.Pawn),
            [new("f7")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = _evaluator.Evaluate(board, endgameFactor: 0).WhiteScore;

        whiteScore.Should().Be(PawnStructureEvaluator.PassedBonus * 4);
    }

    [Fact]
    public void Evaluate_counts_black_passed_pawns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e7")] = PieceFactory.Black(PieceType.Pawn),
            [new("f7")] = PieceFactory.Black(PieceType.Pawn),

            [new("b6")] = PieceFactory.Black(PieceType.Pawn),
            [new("c5")] = PieceFactory.Black(PieceType.Pawn),

            [new("i4")] = PieceFactory.Black(PieceType.Pawn),
            [new("j4")] = PieceFactory.Black(PieceType.Pawn),

            [new("e4")] = PieceFactory.White(PieceType.Pawn),
            [new("f4")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        int blackScore = _evaluator.Evaluate(board, endgameFactor: 0).BlackScore;

        blackScore.Should().Be(PawnStructureEvaluator.PassedBonus * 4);
    }
}
