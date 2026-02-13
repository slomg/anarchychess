using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class MobilityEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_0_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);

        int score = MobilityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_0_when_only_immobile_pieces_exist()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e8")] = PieceFactory.Black(PieceType.King),
            [new("a2")] = PieceFactory.White(PieceType.Pawn),
            [new("a7")] = PieceFactory.Black(PieceType.Pawn),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = MobilityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_rewards_our_piece_with_free_moves()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d4")] = PieceFactory.White(PieceType.Bishop),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = MobilityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Evaluate_penalizes_enemy_piece_with_free_moves()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d4")] = PieceFactory.Black(PieceType.Bishop),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = MobilityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().BeLessThan(0);
    }

    [Fact]
    public void Evaluate_cancels_out_equal_mobility()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("j10")] = PieceFactory.Black(PieceType.Rook),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int score = MobilityEvaluator.Evaluate(board, BitPieceColor.White, BitPieceColor.Black);

        score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_captures_higher_than_quiet_moves()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("j1")] = PieceFactory.Black(PieceType.Pawn),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int scoreWithCapture = MobilityEvaluator.Evaluate(
            board,
            BitPieceColor.White,
            BitPieceColor.Black
        );

        Dictionary<AlgebraicPoint, Piece> noCapturePieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b2")] = PieceFactory.Black(PieceType.Pawn),
        };

        BitBoard boardWithoutCapture = BitBoard.FromPieces(noCapturePieces);

        int scoreWithoutCapture = MobilityEvaluator.Evaluate(
            boardWithoutCapture,
            BitPieceColor.White,
            BitPieceColor.Black
        );

        scoreWithCapture.Should().BeGreaterThan(scoreWithoutCapture);
    }

    [Fact]
    public void Evaluate_is_symmetric_when_colors_are_swapped()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("c3")] = PieceFactory.White(PieceType.Horsey),
            [new("f6")] = PieceFactory.Black(PieceType.Horsey),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        int whiteScore = MobilityEvaluator.Evaluate(
            board,
            BitPieceColor.White,
            BitPieceColor.Black
        );

        int blackScore = MobilityEvaluator.Evaluate(
            board,
            BitPieceColor.Black,
            BitPieceColor.White
        );

        blackScore.Should().Be(-whiteScore);
    }

    [Fact]
    public void Evaluate_blocks_reduce_mobility()
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

        int openScore = MobilityEvaluator.Evaluate(
            BitBoard.FromPieces(openPieces),
            BitPieceColor.White,
            BitPieceColor.Black
        );

        int blockedScore = MobilityEvaluator.Evaluate(
            BitBoard.FromPieces(blockedPieces),
            BitPieceColor.White,
            BitPieceColor.Black
        );

        blockedScore.Should().BeLessThan(openScore);
    }
}
