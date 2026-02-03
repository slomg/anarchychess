using AnarchyChess.Api.Game;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class FenDecoderTests : BaseIntegrationTest
{
    private readonly IFenDecoder _fenDecoder;

    public FenDecoderTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _fenDecoder = Scope.ServiceProvider.GetRequiredService<IFenDecoder>();
    }

    [Fact]
    public void DecodeFen_returns_empty_board_for_all_numbers()
    {
        var result = _fenDecoder.DecodeFen("10/10/10/10/10/10/10/10/10/10");

        result.IsError.Should().BeFalse();

        ChessBoard expectedBoard = new([], height: 10, width: 10);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_returns_correct_pieces_for_starting_position()
    {
        var result = _fenDecoder.DecodeFen(GameTestData.InitialFen);

        result.IsError.Should().BeFalse();

        ChessBoard expectedBoard = new(GameConstants.StartingPosition);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_correctly_handles_non_standard_board_size()
    {
        // 4x3
        var result = _fenDecoder.DecodeFen("2K1/1p+1/4");

        result.IsError.Should().BeFalse();

        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("c3")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("b2")] = new Piece(PieceType.Pawn, GameColor.Black),
            [new AlgebraicPoint("c2")] = new Piece(PieceType.TraitorRook, null),
        };

        ChessBoard expectedBoard = new(pieces, height: 3, width: 4);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_returns_error_for_invalid_piece_letter()
    {
        var result = _fenDecoder.DecodeFen("X3/10/10");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.InvalidPieceLetter);
    }

    [Fact]
    public void DecodeFen_returns_error_for_empty_fen()
    {
        var result = _fenDecoder.DecodeFen("");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenParts);
    }

    [Fact]
    public void DecodeFen_returns_error_for_empty_rank()
    {
        var result = _fenDecoder.DecodeFen("0/0/0");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenPieces);
    }

    [Fact]
    public void DecodeFen_returns_error_for_inconsistent_rank_widths()
    {
        var result = _fenDecoder.DecodeFen("3p/2Q1/1r+/K3");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenPieces);
    }

    [Fact]
    public void DecodeFen_parses_black_side_to_move()
    {
        var fen = "10/10/10/10 {\"sideToMove\":1}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();
        result.Value.SideToMove.Should().Be(GameColor.Black);
    }

    [Fact]
    public void DecodeFen_sets_moved_flag_correctly_for_moved_pieces()
    {
        var fen = "K9/r9 {\"movedPieces\":[\"a1\",\"b1\"]}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();

        var board = result.Value;
        board.PeekPieceAt(new AlgebraicPoint("a1"))?.HasMoved.Should().BeTrue();
        board.PeekPieceAt(new AlgebraicPoint("b1"))?.HasMoved.Should().BeTrue();
    }

    [Fact]
    public void DecodeFen_handles_no_moved_pieces_indicator()
    {
        var fen = "K9/r9 {}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();
        result.Value.EnumeratePieces().All(x => x.Occupant.HasMoved == false).Should().BeTrue();
    }

    [Fact]
    public void DecodeFen_returns_error_for_moved_piece_outside_board()
    {
        var fen = "K9/r9 {\"movedPieces\":[\"z9\"]}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenMovedPieces);
    }

    [Fact]
    public void DecodeFen_parses_last_move_correctly()
    {
        var fen = "1k8/10 {\"lastMove\":{\"from\":\"a1\",\"to\":\"b2\"}}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();

        var board = result.Value;
        board.Moves.Should().HaveCount(1);

        var movedPiece = board.PeekPieceAt(new("b2"));
        movedPiece.Should().NotBeNull();
        board
            .Moves[0]
            .Should()
            .BeEquivalentTo(new Move(from: new("a1"), to: new("b2"), piece: movedPiece));

        board.PeekPieceAt(new AlgebraicPoint("b2"))?.HasMoved.Should().BeFalse(); // only movedPieces decides HasMoved
    }

    [Fact]
    public void DecodeFen_parses_last_move_with_captures()
    {
        var fen =
            "Pp {\"lastMove\":{\"from\":\"a1\",\"to\":\"b1\",\"captures\":[{\"piece\":{\"type\":2,\"color\":1,\"hasMoved\":false},\"pos\":\"b1\"}]}}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();

        var board = result.Value;
        board.Moves.Should().HaveCount(1);

        var movedPiece = board.PeekPieceAt(new("b1"));
        movedPiece.Should().NotBeNull();
        board
            .Moves[0]
            .Should()
            .BeEquivalentTo(
                new Move(
                    from: new("a1"),
                    to: new("b1"),
                    piece: movedPiece,
                    captures:
                    [
                        new MoveCapture(
                            CapturedPiece: new Piece(Type: PieceType.Pawn, Color: GameColor.Black),
                            Position: new("b1")
                        ),
                    ]
                )
            );
    }

    [Fact]
    public void DecodeFen_handles_no_last_move()
    {
        var fen = "K9/r9 {}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();
        result.Value.Moves.Should().BeEmpty();
    }

    [Fact]
    public void DecodeFen_returns_error_for_invalid_last_move_capture()
    {
        var fen =
            "Pp {\"lastMove\":{\"from\":\"a1\",\"to\":\"b1\",\"captures\":[{\"piece\":{\"type\":2,\"color\":1,\"hasMoved\":false},\"pos\":\"z9\"}]}}"; // invalid pos
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenLastMove);
    }

    [Fact]
    public void DecodeFen_returns_error_for_last_move_outside_board()
    {
        var fen = "K9/r9 {\"lastMove\":{\"from\":\"z9\",\"to\":\"a1\"}}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFenLastMove);
    }

    [Fact]
    public void DecodeFen_parses_halfmove_clock_correctly()
    {
        var fen = "K9/r9 {\"halfMoveClock\":42}";
        var result = _fenDecoder.DecodeFen(fen);

        result.IsError.Should().BeFalse();
        result.Value.HalfMoveClock.Should().Be(42);
    }
}
