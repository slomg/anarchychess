using AnarchyChess.Api.Game;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class FenEncoderTests : BaseIntegrationTest
{
    private readonly IFenEncoder _fenEncoder;

    public FenEncoderTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _fenEncoder = Scope.ServiceProvider.GetRequiredService<IFenEncoder>();
    }

    [Fact]
    public void EncodeFen_returns_correct_fennotation_record()
    {
        ChessBoard board = new(
            GameConstants.StartingPosition,
            height: 10,
            width: 10,
            sideToMove: GameColor.Black,
            halfMoveClock: 5
        );

        var result = _fenEncoder.EncodeFen(board);

        result.Position.Should().Be(GameTestData.InitialFen);
        result
            .FullFen.Should()
            .Be(GameTestData.InitialFen + " {\"sideToMove\":1,\"halfMoveClock\":5}");
    }

    [Fact]
    public void EncodeFen_returns_all_numbers_for_empty_board()
    {
        ChessBoard board = new([], height: 10, width: 10);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("10/10/10/10/10/10/10/10/10/10");
    }

    [Fact]
    public void EncodeFen_returns_the_correct_fen_for_the_starting_position()
    {
        ChessBoard board = new(GameConstants.StartingPosition);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be(GameTestData.InitialFen);
    }

    [Fact]
    public void EncodeFen_correctly_compresses_rows_with_different_piece_colors()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("d1")] = new Piece(PieceType.Rook, GameColor.Black),
            [new AlgebraicPoint("g1")] = new Piece(PieceType.Queen, GameColor.White),
            [new AlgebraicPoint("c2")] = new Piece(PieceType.Pawn, GameColor.White),
        };
        ChessBoard board = new(pieces);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("10/10/10/10/10/10/10/10/2P7/K2r2Q3");
    }

    [Fact]
    public void EncodeFen_correctly_places_a_piece_on_all_rows()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("b2")] = new Piece(PieceType.Rook, GameColor.Black),
            [new AlgebraicPoint("c3")] = new Piece(PieceType.Queen, GameColor.White),
            [new AlgebraicPoint("d4")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        ChessBoard board = new(pieces, height: 4, width: 4);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("3p/2Q1/1r2/K3");
    }

    [Fact]
    public void EncodeFen_returns_correct_fen_with_side_to_move_black()
    {
        ChessBoard board = new([], height: 2, width: 2, sideToMove: GameColor.Black);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("2/2 {\"sideToMove\":1}");
    }

    [Fact]
    public void EncodeFen_returns_correct_fen_with_moved_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.Pawn, GameColor.White, HasMoved: true),
            [new AlgebraicPoint("a2")] = new Piece(
                PieceType.UnderagePawn,
                GameColor.Black,
                HasMoved: true
            ),
            [new AlgebraicPoint("a3")] = new Piece(
                PieceType.SterilePawn,
                GameColor.White,
                HasMoved: true
            ),
            [new AlgebraicPoint("a4")] = new Piece(PieceType.King, GameColor.Black, HasMoved: true),
            [new AlgebraicPoint("a5")] = new Piece(PieceType.Rook, GameColor.White, HasMoved: true),

            [new AlgebraicPoint("b1")] = new Piece(PieceType.King, GameColor.Black),
            [new AlgebraicPoint("b2")] = new Piece(PieceType.Rook, GameColor.White),
            [new AlgebraicPoint("b3")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        ChessBoard board = new(pieces, height: 5, width: 2);

        var result = _fenEncoder.EncodeFen(board);

        result
            .FullFen.Should()
            .Be("R1/k1/Sp/dR/Pk {\"movedPieces\":[\"a1\",\"a2\",\"a3\",\"a4\",\"a5\"]}");
    }

    [Fact]
    public void EncodeFen_returns_correct_fen_with_last_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.Pawn, GameColor.White),
            [new AlgebraicPoint("b1")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        List<Move> moves = [new Move(from: new("a1"), to: new("a2"), piece: pieces[new("a1")])];
        ChessBoard board = new(pieces, height: 2, width: 2, moves: moves);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("2/Pp {\"lastMove\":{\"from\":\"a1\",\"to\":\"a2\"}}");
    }

    [Fact]
    public void EncodeFen_returns_correct_fen_with_captures_in_last_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.Pawn, GameColor.White),
            [new AlgebraicPoint("b1")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        List<Move> moves =
        [
            new Move(
                from: new("a1"),
                to: new("b1"),
                piece: pieces[new("a1")],
                captures: [new MoveCapture(CapturedPiece: pieces[new("b1")], Position: new("b1"))]
            ),
        ];
        ChessBoard board = new(pieces, height: 1, width: 2, moves: moves);

        var result = _fenEncoder.EncodeFen(board);

        result
            .FullFen.Should()
            .Be(
                "Pp {\"lastMove\":{\"from\":\"a1\",\"to\":\"b1\",\"captures\":[{\"piece\":{\"type\":2,\"color\":1,\"hasMoved\":false},\"pos\":\"b1\"}]}}"
            );
    }

    [Fact]
    public void EncodeFen_returns_correct_fen_with_stunned_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.Pawn, GameColor.White),
            [new AlgebraicPoint("b1")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        ChessBoard board = new(
            pieces,
            height: 1,
            width: 2,
            stunnedPieces: new() { [new("a2")] = 69 }
        );

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("Pp {\"stunnedPieces\":{\"a2\":69}}");
    }

    [Fact]
    public void EncodeFen_returns_correct_fen_with_halfmove_clock()
    {
        ChessBoard board = new([], height: 2, width: 2, halfMoveClock: 42);

        var result = _fenEncoder.EncodeFen(board);

        result.FullFen.Should().Be("2/2 {\"halfMoveClock\":42}");
    }
}
