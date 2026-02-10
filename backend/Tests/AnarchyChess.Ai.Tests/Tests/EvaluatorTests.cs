using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class EvaluatorTests
{
    private readonly Evaluator _evaluator = new();

    [Fact]
    public void EvaluateBoard_returns_positive_score_when_only_our_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        float expected = 5 + 3.5f;
        score.Should().Be(expected);
    }

    [Fact]
    public void EvaluateBoard_returns_negative_score_when_only_enemy_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.Black(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        float expected = -(5 + 3.5f);
        score.Should().Be(expected);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_0_without_adjacent_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("h2")] = PieceFactory.White(PieceType.Rook),
            [new("b8")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        score.Should().Be(0f);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_2_when_we_have_more_adjacent()
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

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        score.Should().Be(2f);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_1_when_adjacent_is_equal()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        score.Should().Be(1f);
    }

    [Fact]
    public void EvaluateBoard_TraitorRook_more_enemy_adjacent_returns_negative()
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

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        score.Should().Be(-2f);
    }

    [Fact]
    public void EvaluateBoard_sums_own_and_enemy_pieces_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Checker),
            [new("c1")] = PieceFactory.Black(PieceType.Queen),
            [new("d1")] = PieceFactory.Black(PieceType.UnderagePawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        // own pieces: 5 + 3.5 = 8.5
        // enemy pieces: 9 + 1.5 = 10.5
        // total = 8.5 - 10.5 = -2
        score.Should().Be(-2);
    }

    [Fact]
    public void EvaluateBoard_handles_multiple_kings_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("f1")] = PieceFactory.White(PieceType.King),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, BitPieceColor.White);

        // EvaluateKingScore for White: 10_000 / 2 + 3.5 = 5003.5 per king, total = 5003.5 * 2 = 10007
        // EvaluateKingScore for Black: 10_000 / 1 + 3.5 = 10003.5
        // total score = 10007 - 10003.5 = 3.5
        score.Should().Be(3.5f);
    }
}
