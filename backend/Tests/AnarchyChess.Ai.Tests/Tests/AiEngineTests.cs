using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class AiEngineTests
{
    private readonly AiEngine _engine = new();

    [Fact]
    public void FindBestMove_returns_null_on_empty_board()
    {
        BitBoard board = new();

        var move = _engine.FindBestMove(board, depth: 1).BestMove;

        move.Should().BeNull();
    }

    [Fact]
    public void FindBestMove_prefers_capture_over_non_capture()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Pawn),
            [new("e5")] = PieceFactory.White(PieceType.King),
            [new("g5")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (BitMove? move, int evalForBot) = _engine.FindBestMove(board, depth: 1);

        move.Should().NotBeNull();
        move.Value.From.Should().Be(new AlgebraicPoint("a1").AsIdx());
        move.Value.To.Should().Be(new AlgebraicPoint("b1").AsIdx());
        evalForBot.Should().BeGreaterThan(0);
    }

    [Fact]
    public void FindBestMove_prefers_highest_value_capture()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Pawn),
            [new("a5")] = PieceFactory.Black(PieceType.Queen),
            [new("e7")] = PieceFactory.White(PieceType.King),
            [new("g7")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove? move = _engine.FindBestMove(board, depth: 1).BestMove;

        move.Should().NotBeNull();
        move.Value.To.Should().Be(new AlgebraicPoint("a5").AsIdx());
    }

    [Fact]
    public void FindBestMove_returns_negative_eval_when_bot_is_losing()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.Black(PieceType.Queen),
            [new("b1")] = PieceFactory.Black(PieceType.Queen),
            [new("c1")] = PieceFactory.Black(PieceType.Queen),

            [new("g3")] = PieceFactory.Black(PieceType.King),
            [new("j3")] = PieceFactory.White(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (BitMove? move, int evalForBot) = _engine.FindBestMove(board, depth: 1);

        move.Should().NotBeNull();
        evalForBot.Should().BeLessThan(0);
    }

    [Fact]
    public void FindBestMove_black_to_move_prefers_capture_and_returns_positive_eval()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a10")] = PieceFactory.Black(PieceType.Rook),
            [new("a1")] = PieceFactory.White(PieceType.Queen),

            [new("e10")] = PieceFactory.Black(PieceType.King),
            [new("e1")] = PieceFactory.White(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        (BitMove? move, int evalForBot) = _engine.FindBestMove(board, depth: 1);

        move.Should().NotBeNull();

        move.Value.From.Should().Be(new AlgebraicPoint("a10").AsIdx());
        move.Value.To.Should().Be(new AlgebraicPoint("a1").AsIdx());
        evalForBot.Should().BeGreaterThan(0);
    }

    [Fact]
    public void EvaluateAllMoves_returns_empty_on_empty_board()
    {
        BitBoard board = new();

        MoveEvaluation[] evaluations = _engine.EvaluateAllMoves(board, depth: 1);

        evaluations.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateAllMoves_scores_captures_higher_than_non_captures()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Knook),
            [new("h1")] = PieceFactory.Black(PieceType.Horsey),
            [new("e5")] = PieceFactory.White(PieceType.King),
            [new("g5")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        MoveEvaluation[] evaluations = _engine.EvaluateAllMoves(board, depth: 1);

        var knookCapture = evaluations.First(x =>
            x.Move.From == new AlgebraicPoint("e1").AsIdx()
            && x.Move.To == new AlgebraicPoint("b1").AsIdx()
        );

        var horseyCapture = evaluations.First(x =>
            x.Move.From == new AlgebraicPoint("e1").AsIdx()
            && x.Move.To == new AlgebraicPoint("h1").AsIdx()
        );

        knookCapture.EvalForBot.Should().BeGreaterThan(horseyCapture.EvalForBot);
    }
}
