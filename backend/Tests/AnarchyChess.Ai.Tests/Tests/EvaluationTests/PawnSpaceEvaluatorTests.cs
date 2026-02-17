using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class PawnSpaceEvaluatorTests
{
    private readonly PawnSpaceEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_single_white_pawn_progress_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a5")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int rank = 4;
        int distance = Math.Abs(rank - 9);
        int progress = 10 - distance;

        int expectedWhite = progress * PawnSpaceEvaluator.PawnAdvanceValue * 1;

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, 0);

        whiteScore.Should().Be(expectedWhite);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_applies_center_amplifier_for_white_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int rank = 4;
        int distance = Math.Abs(rank - 9);
        int progress = 10 - distance;

        int expectedWhite =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, 0);

        whiteScore.Should().Be(expectedWhite);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_includes_underage_pawns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d6")] = PieceFactory.White(PieceType.UnderagePawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int rank = 5;
        int distance = Math.Abs(rank - 9);
        int progress = 10 - distance;

        int expectedWhite =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, 0);

        whiteScore.Should().Be(expectedWhite);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_black_pawn_space_separately()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int rank = 4;
        int distance = Math.Abs(rank - 0);
        int progress = 10 - distance;

        int expectedBlack =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(expectedBlack);
    }

    [Fact]
    public void Evaluate_sums_multiple_white_pawns_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d6")] = PieceFactory.White(PieceType.Pawn),
            [new("f7")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int expectedWhite = 0;
        foreach (int rank in new int[] { 5, 6 })
        {
            int distance = Math.Abs(rank - 9);
            int progress = 10 - distance;
            expectedWhite +=
                progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;
        }

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, 0);

        whiteScore.Should().Be(expectedWhite);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_sums_multiple_black_pawns_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d4")] = PieceFactory.Black(PieceType.Pawn),
            [new("f3")] = PieceFactory.Black(PieceType.UnderagePawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int expectedBlack = 0;
        foreach (int rank in new int[] { 3, 2 })
        {
            int distance = Math.Abs(rank - 0);
            int progress = 10 - distance;
            expectedBlack +=
                progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;
        }

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(expectedBlack);
    }
}
