using AnarchyChess.Api.Game;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class FenCalculatorTests : BaseIntegrationTest
{
    private readonly IFenCalculator _fenCalculator;

    private const string InitialFen =
        "rhnbqkbcar/pppdppdppp/10/10/9+/+9/10/10/PPPDPPDPPP/RHNBQKBCAR";

    public FenCalculatorTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _fenCalculator = Scope.ServiceProvider.GetRequiredService<IFenCalculator>();
    }

    [Fact]
    public void CalculateFen_returns_all_numbers_for_empty_board()
    {
        ChessBoard board = new([], height: 10, width: 10);

        var result = _fenCalculator.CalculateFen(board);

        result.Should().Be("10/10/10/10/10/10/10/10/10/10");
    }

    [Fact]
    public void CalculateFen_returns_the_correct_fen_for_the_starting_position()
    {
        ChessBoard board = new(GameConstants.StartingPosition);

        var result = _fenCalculator.CalculateFen(board);

        result.Should().Be(InitialFen);
    }

    [Fact]
    public void CalculateFen_correctly_compresses_rows_with_different_piece_colors()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("d1")] = new Piece(PieceType.Rook, GameColor.Black),
            [new AlgebraicPoint("g1")] = new Piece(PieceType.Queen, GameColor.White),
            [new AlgebraicPoint("c2")] = new Piece(PieceType.Pawn, GameColor.White),
        };
        ChessBoard board = new(pieces);

        var result = _fenCalculator.CalculateFen(board);

        result.Should().Be("10/10/10/10/10/10/10/10/2P7/K2r2Q3");
    }

    [Fact]
    public void CalculateFen_correctly_places_a_piece_on_all_rows()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("b2")] = new Piece(PieceType.Rook, GameColor.Black),
            [new AlgebraicPoint("c3")] = new Piece(PieceType.Queen, GameColor.White),
            [new AlgebraicPoint("d4")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        ChessBoard board = new(pieces, height: 4, width: 4);

        var result = _fenCalculator.CalculateFen(board);

        result.Should().Be("3p/2Q1/1r2/K3");
    }

    [Fact]
    public void DecodeFen_returns_empty_board_for_all_numbers()
    {
        var result = _fenCalculator.DecodeFen("10/10/10/10/10/10/10/10/10/10");

        result.IsError.Should().BeFalse();

        ChessBoard expectedBoard = new([], height: 10, width: 10);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_returns_correct_pieces_for_starting_position()
    {
        var result = _fenCalculator.DecodeFen(InitialFen);

        result.IsError.Should().BeFalse();

        ChessBoard expectedBoard = new(GameConstants.StartingPosition);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_sets_sideToMove_correctly()
    {
        var result = _fenCalculator.DecodeFen(
            "10/10/10/10/10/10/10/10/10/10",
            sideToMove: GameColor.Black
        );

        result.IsError.Should().BeFalse();

        ChessBoard expectedBoard = new([], height: 10, width: 10, sideToMove: GameColor.Black);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_correctly_handles_non_standard_board_size()
    {
        // 4x3
        var result = _fenCalculator.DecodeFen("2K1/1p+1/4");

        result.IsError.Should().BeFalse();

        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint(2, 2)] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint(1, 1)] = new Piece(PieceType.Pawn, GameColor.Black),
            [new AlgebraicPoint(2, 1)] = new Piece(PieceType.Rook, null),
        };

        ChessBoard expectedBoard = new(pieces, height: 3, width: 4);
        result.Value.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void DecodeFen_returns_error_for_invalid_piece_letter()
    {
        var result = _fenCalculator.DecodeFen("X3/10/10");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.InvalidPieceLetter);
    }

    [Fact]
    public void DecodeFen_returns_error_for_empty_fen()
    {
        var result = _fenCalculator.DecodeFen("");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFen);
    }

    [Fact]
    public void DecodeFen_returns_error_for_empty_rank()
    {
        var result = _fenCalculator.DecodeFen("0/0/0");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFen);
    }

    [Fact]
    public void DecodeFen_returns_error_for_inconsistent_rank_widths()
    {
        var result = _fenCalculator.DecodeFen("3p/2Q1/1r+/K3");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MalformedFen);
    }
}
