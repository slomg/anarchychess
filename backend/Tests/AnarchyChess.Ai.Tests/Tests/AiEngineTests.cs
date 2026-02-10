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

        var move = _engine.FindBestMove(board, depth: 1);

        move.Should().BeNull();
    }

    [Fact]
    public void FindBestMove_prefers_capture_over_non_capture()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove? move = _engine.FindBestMove(board, depth: 1);

        move.Should().NotBeNull();
        move.Value.To.Should().Be(new AlgebraicPoint("b1").AsIdx());
    }

    [Fact]
    public void FindBestMove_prefers_highest_value_capture()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Pawn),
            [new("a5")] = PieceFactory.Black(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove? move = _engine.FindBestMove(board, depth: 1);

        move.Should().NotBeNull();
        move.Value.To.Should().Be(new AlgebraicPoint("a5").AsIdx());
    }
}
