using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitMoveGeneratorTests
{
    private readonly BitMovesGenerator _generator = new();

    [Fact]
    public void Generate_returns_expected_moves_for_white()
    {
        var whiteKing = PieceFactory.White(PieceType.King);
        var whitePawn = PieceFactory.White(PieceType.Pawn, hasMoved: false);

        var board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new AlgebraicPoint("f5")] = whiteKing,
                [new AlgebraicPoint("d2")] = whitePawn,
                [new AlgebraicPoint("b4")] = PieceFactory.Black(PieceType.King),
            },
            isWhiteToMove: true
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        HashSet<byte> expectedDestinations =
        [
            .. new AlgebraicPoint[]
            {
                // king
                new("e4"),
                new("e5"),
                new("e6"),
                new("f4"),
                new("f6"),
                new("g4"),
                new("g5"),
                new("g6"),
                // pawn
                new("d3"),
                new("d4"),
                new("d5"),
            }.Select(p => p.AsIdx()),
        ];

        moveCount.Should().Be(expectedDestinations.Count);
        foreach (var move in moves[..moveCount])
        {
            expectedDestinations.Should().Contain(move.To);
        }
    }

    [Fact]
    public void Generate_returns_expected_moves_for_black()
    {
        var blackPawn = PieceFactory.Black(PieceType.Pawn, hasMoved: false);

        var board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new AlgebraicPoint("d9")] = blackPawn,
                [new AlgebraicPoint("f2")] = PieceFactory.White(PieceType.King),
            },
            isWhiteToMove: false
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        HashSet<byte> expectedDestinations =
        [
            .. new AlgebraicPoint[] { new("d8"), new("d7"), new("d6") }.Select(p => p.AsIdx()),
        ];

        moveCount.Should().Be(expectedDestinations.Count);
        foreach (var move in moves[..moveCount])
        {
            expectedDestinations.Should().Contain(move.To);
        }
    }
}
