namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

using System.Collections.Generic;
using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Xunit;

public class EndgameFactorCalculatorTests
{
    private readonly EndgameFactorCalculator _calculator = new();

    [Fact]
    public void EndgameFactor_returns_1_when_board_is_empty()
    {
        BitBoard board = new();
        _calculator.EndgameFactor(board).Should().Be(1f);
    }

    [Fact]
    public void EndgameFactor_returns_0_when_all_heavy_pieces_are_present()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Queen),
            [new("a2")] = PieceFactory.Black(PieceType.Queen),
            [new("a3")] = PieceFactory.White(PieceType.Rook),
            [new("a4")] = PieceFactory.Black(PieceType.Rook),
            [new("a5")] = PieceFactory.White(PieceType.Rook),
            [new("a6")] = PieceFactory.Black(PieceType.Rook),
            [new("a7")] = PieceFactory.White(PieceType.Knook),
            [new("a8")] = PieceFactory.Black(PieceType.Knook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        _calculator.EndgameFactor(board).Should().Be(0f);
    }

    [Fact]
    public void EndgameFactor_returns_between_0_and_1_with_only_queens()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Queen),
            [new("h8")] = PieceFactory.Black(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        _calculator.EndgameFactor(board).Should().BeGreaterThan(0f).And.BeLessThan(1f);
    }

    [Fact]
    public void EndgameFactor_returns_small_value_with_only_knooks()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Knook),
            [new("h8")] = PieceFactory.Black(PieceType.Knook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        _calculator
            .EndgameFactor(board)
            .Should()
            .Be(1f - (2 * 0.5f / EndgameFactorCalculator.MaxPhase));
    }

    [Fact]
    public void EndgameFactor_clamps_to_0_when_phase_exceeds_max()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Queen),
            [new("b1")] = PieceFactory.White(PieceType.Rook),
            [new("a2")] = PieceFactory.White(PieceType.Queen),
            [new("b2")] = PieceFactory.White(PieceType.Rook),
            [new("a3")] = PieceFactory.White(PieceType.Queen),
            [new("b3")] = PieceFactory.White(PieceType.Rook),
            [new("a4")] = PieceFactory.White(PieceType.Queen),
            [new("b4")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        _calculator.EndgameFactor(board).Should().Be(0f);
    }
}
