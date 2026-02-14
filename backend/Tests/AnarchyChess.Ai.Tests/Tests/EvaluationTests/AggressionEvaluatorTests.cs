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
        BitBoard board = BitBoard.FromPieces([]);
        AggressionEvaluator.Evaluate(board).Should().Be(0);
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

        AggressionEvaluator.Evaluate(board).Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_single_piece_aggression()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("d7")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        // distance from d7 (file 3, rank 6) to e8 (file 4, rank 7) = |3-4| + |6-7| = 2
        AggressionEvaluator.Evaluate(board).Should().Be(AggressionEvaluator.MaxDistanceBonus - 2);
    }

    [Fact]
    public void Evaluate_subtracts_enemy_aggression_against_our_king()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("d2")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        // distance from d2 (file 3, rank 1) to e1 (file 4, rank 0) = |3-4| + |1-0| = 2
        AggressionEvaluator
            .Evaluate(board)
            .Should()
            .Be(-(AggressionEvaluator.MaxDistanceBonus - 2));
    }

    [Fact]
    public void Evaluate_combines_both_sides_aggression()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("d7")] = PieceFactory.White(PieceType.Rook),
            [new("d2")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        // white rook to black king: 18
        // wlack rook to white king: -18
        AggressionEvaluator.Evaluate(board).Should().Be(0);
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
        // d7 -> e8 = 2
        // f7 -> e8 = 2
        // Black pieces distances to white king:
        // d2 -> e1 = 2
        // c3 -> e1 = 4
        AggressionEvaluator.Evaluate(board).Should().Be(2);
    }
}
