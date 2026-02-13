using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class ActivityEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_0_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);

        int score = ActivityEvaluator.Evaluate(
            board,
            ourColor: BitPieceColor.White,
            enemyColor: BitPieceColor.Black
        );

        score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_single_our_horsey_from_table()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Horsey),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]);
    }

    [Fact]
    public void Evaluate_scores_single_enemy_horsey_as_negative_table_value()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Black(PieceType.Horsey),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(-ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]);
    }

    [Fact]
    public void Evaluate_cancels_out_equal_our_and_enemy_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d5")] = PieceFactory.White(PieceType.Bishop),
            [new("d6")] = PieceFactory.Black(PieceType.Bishop),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(0);
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

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score
            .Should()
            .Be(
                ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]
                    + ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("d4").AsIdx()]
                    + ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("f4").AsIdx()]
            );
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

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score
            .Should()
            .Be(
                ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]
                    + ActivityEvaluator.BishopActivityTable[new AlgebraicPoint("c6").AsIdx()]
            );
    }

    [Fact]
    public void Evaluate_mixes_our_and_enemy_piece_types_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Horsey),
            [new("a1")] = PieceFactory.Black(PieceType.Checker),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score
            .Should()
            .Be(
                ActivityEvaluator.HorseyActivityTable[new AlgebraicPoint("e5").AsIdx()]
                    - ActivityEvaluator.CheckerActivityTable[new AlgebraicPoint("a1").AsIdx()]
            );
    }

    [Fact]
    public void Evaluate_is_symmetric_when_colors_are_swapped()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Bishop),
            [new("a1")] = PieceFactory.Black(PieceType.Bishop),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = ActivityEvaluator.Evaluate(
            board,
            BitPieceColor.White,
            BitPieceColor.Black
        );
        int blackScore = ActivityEvaluator.Evaluate(
            board,
            BitPieceColor.Black,
            BitPieceColor.White
        );

        blackScore.Should().Be(-whiteScore);
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

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Evaluate_ignores_non_activity_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Pawn),
            [new("f6")] = PieceFactory.White(PieceType.King),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = ActivityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(0);
    }
}
