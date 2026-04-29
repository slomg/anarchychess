using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class MaterialEvaluatorTests
{
    [Fact]
    public void EvaluateBoard_counts_white_material_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(850);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void EvaluateBoard_counts_black_material_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.Black(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(850);
    }

    [Fact]
    public void EvaluateBoard_traitor_rook_counts_as_150_when_adjacent_to_white_majority()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
            [new("e4")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.Black(PieceType.Pawn),
            [new("a1")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be((100 * 2) + 150);
        evaluation.BlackScore.Should().Be((100 * 2) + 0);
    }

    [Fact]
    public void EvaluateBoard_traitor_rook_counts_to_white_when_adjacent_equal_and_closer_to_white()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook), // position < 50
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(250);
        evaluation.BlackScore.Should().Be(100);
    }

    [Fact]
    public void EvaluateBoard_traitor_rook_counts_to_white_when_no_adjacent_pieces_and_closer_to_white()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("h2")] = PieceFactory.White(PieceType.Rook),
            [new("b8")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(650);
        evaluation.BlackScore.Should().Be(500);
    }

    [Fact]
    public void EvaluateBoard_traitor_rook_counts_to_black_when_adjacent_equal_and_closer_to_black()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e8")] = PieceFactory.Neutral(PieceType.TraitorRook), // position >= 50
            [new("d8")] = PieceFactory.White(PieceType.Pawn),
            [new("f8")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(100);
        evaluation.BlackScore.Should().Be(250);
    }

    [Fact]
    public void EvaluateBoard_traitor_rook_counts_to_black_when_no_adjacent_pieces_and_closer_to_black_side()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e8")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("h2")] = PieceFactory.White(PieceType.Rook),
            [new("b8")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(500);
        evaluation.BlackScore.Should().Be(650);
    }

    [Fact]
    public void EvaluateBoard_traitor_rook_counts_as_150_when_adjacent_to_black_majority()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("d5")] = PieceFactory.Black(PieceType.Pawn),
            [new("e4")] = PieceFactory.Black(PieceType.Pawn),
            [new("f5")] = PieceFactory.White(PieceType.Pawn),
            [new("a1")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be((100 * 2) + 0);
        evaluation.BlackScore.Should().Be((100 * 2) + 150);
    }

    [Fact]
    public void EvaluateBoard_sums_white_and_black_material_independently()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Checker),
            [new("c1")] = PieceFactory.Black(PieceType.Queen),
            [new("d1")] = PieceFactory.Black(PieceType.UnderagePawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(850);
        evaluation.BlackScore.Should().Be(1150);
    }

    [Fact]
    public void EvaluateBoard_counts_black_material_correctly_when_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Rook),
            [new("c1")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        EvaluationResult evaluation = MaterialEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(1000);
        evaluation.BlackScore.Should().Be(500);
    }
}
