using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class ActivityEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_0_on_empty_board()
    {
        BitBoard board = new();

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_white_horsey_from_table()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new("e5")] = PieceFactory.White(PieceType.Horsey),
            }
        );

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation
            .WhiteScore.Should()
            .Be(ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_black_horsey_from_table()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Black(PieceType.Horsey),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation
            .BlackScore.Should()
            .Be(ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]);
    }

    [Fact]
    public void Evaluate_gives_the_same_score_when_material_is_equal()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d5")] = PieceFactory.White(PieceType.Bishop),
            [new("d6")] = PieceFactory.Black(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation
            .WhiteScore.Should()
            .Be(ActivityEvaluator.BishopActivityTable[new AlgebraicPoint("d5").AsIdx()]);
        evaluation.BlackScore.Should().Be(evaluation.WhiteScore);
    }

    [Fact]
    public void Evaluate_sums_multiple_horsey_like_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Horsey),
            [new("d4")] = PieceFactory.White(PieceType.Knook),
            [new("f4")] = PieceFactory.White(PieceType.Antiqueen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation
            .WhiteScore.Should()
            .Be(
                ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]
                    + ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("d4").AsIdx()]
                    + ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("f4").AsIdx()]
            );
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_uses_correct_table_per_piece_type()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Horsey),
            [new("c6")] = PieceFactory.White(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation
            .WhiteScore.Should()
            .Be(
                ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]
                    + ActivityEvaluator.BishopActivityTable[new AlgebraicPoint("c6").AsIdx()]
            );
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_evalutes_both_colors()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Horsey),
            [new("a1")] = PieceFactory.Black(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation
            .WhiteScore.Should()
            .Be(ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]);
        evaluation
            .BlackScore.Should()
            .Be(ActivityEvaluator.CheckerActivityTable[new AlgebraicPoint("a1").AsIdx()]);
    }

    [Fact]
    public void Evaluate_rewards_center_over_corner()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Horsey),
            [new("a1")] = PieceFactory.Black(PieceType.Horsey),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().BeGreaterThan(evaluation.BlackScore);
    }

    [Fact]
    public void Evaluate_ignores_non_activity_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = ActivityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
    }
}
