using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class PawnSpaceEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);

        PawnSpaceEvaluator.Evaluate(board).Should().Be(0);
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

        int expected = progress * PawnSpaceEvaluator.PawnAdvanceValue * 1;

        PawnSpaceEvaluator.Evaluate(board).Should().Be(expected);
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

        int expected =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        PawnSpaceEvaluator.Evaluate(board).Should().Be(expected);
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

        int expected =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        PawnSpaceEvaluator.Evaluate(board).Should().Be(expected);
    }

    [Fact]
    public void Evaluate_subtracts_black_pawn_space_when_white_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: true);

        int rank = 4;
        int distance = Math.Abs(rank - 0);
        int progress = 10 - distance;

        int blackScore =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        PawnSpaceEvaluator.Evaluate(board).Should().Be(-blackScore);
    }

    [Fact]
    public void Evaluate_flips_sign_when_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        int rank = 4;
        int distance = Math.Abs(rank - 9);
        int progress = 10 - distance;

        int whiteScore =
            progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;

        PawnSpaceEvaluator.Evaluate(board).Should().Be(-whiteScore);
    }

    [Fact]
    public void Evaluate_sums_multiple_pawns_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d6")] = PieceFactory.White(PieceType.Pawn),
            [new("f7")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int expected = 0;
        foreach (int rank in new int[] { 5, 6 })
        {
            int distance = Math.Abs(rank - 9);
            int progress = 10 - distance;

            expected +=
                progress * PawnSpaceEvaluator.PawnAdvanceValue * PawnSpaceEvaluator.CenterAmplifier;
        }

        PawnSpaceEvaluator.Evaluate(board).Should().Be(expected);
    }
}
