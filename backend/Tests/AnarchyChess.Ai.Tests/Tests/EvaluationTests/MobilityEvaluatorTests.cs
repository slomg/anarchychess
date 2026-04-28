using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class MobilityEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = new();

        EvaluationResult evaluation = MobilityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_zero_when_pieces_have_no_moves()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("a2")] = PieceFactory.White(PieceType.Pawn),
            [new("a7")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MobilityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_white_piece_mobility()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d4")] = PieceFactory.White(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MobilityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().BeGreaterThan(0);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_black_piece_mobility()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d4")] = PieceFactory.Black(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = MobilityEvaluator.Evaluate(board);

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Evaluate_counts_captures_higher_than_quiet_moves()
    {
        Dictionary<AlgebraicPoint, Piece> piecesWithCapture = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("j1")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard boardWithCapture = BitBoard.FromPieces(piecesWithCapture);

        int scoreWithCapture = MobilityEvaluator.Evaluate(boardWithCapture).WhiteScore;

        Dictionary<AlgebraicPoint, Piece> piecesWithoutCapture = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b2")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard boardWithoutCapture = BitBoard.FromPieces(piecesWithoutCapture);

        int scoreWithoutCapture = MobilityEvaluator.Evaluate(boardWithoutCapture).WhiteScore;

        scoreWithCapture.Should().BeGreaterThan(scoreWithoutCapture);
    }

    [Fact]
    public void Evaluate_counts_open_and_blocked_pieces_separately()
    {
        Dictionary<AlgebraicPoint, Piece> openPieces = new()
        {
            [new("d4")] = PieceFactory.White(PieceType.Rook),
        };
        Dictionary<AlgebraicPoint, Piece> blockedPieces = new()
        {
            [new("d4")] = PieceFactory.White(PieceType.Rook),
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
        };

        int openScore = MobilityEvaluator.Evaluate(BitBoard.FromPieces(openPieces)).WhiteScore;
        int blockedScore = MobilityEvaluator
            .Evaluate(BitBoard.FromPieces(blockedPieces))
            .WhiteScore;

        openScore.Should().BeGreaterThan(0);
        blockedScore.Should().BeGreaterThan(0);
    }
}
