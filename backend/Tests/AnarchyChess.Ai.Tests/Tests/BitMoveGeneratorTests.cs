using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitMoveGeneratorTests
{
    private readonly BitMovesGenerator _generator = new();

    [Fact]
    public void Generate_returns_expected_moves_for_king()
    {
        var king = PieceFactory.White(PieceType.King);
        var board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece> { [new AlgebraicPoint("f5")] = king }
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        moveCount.Should().Be(8);

        var expectedDestinations = new[]
        {
            new AlgebraicPoint("e4"),
            new AlgebraicPoint("e5"),
            new AlgebraicPoint("e6"),
            new AlgebraicPoint("f4"),
            new AlgebraicPoint("f6"),
            new AlgebraicPoint("g4"),
            new AlgebraicPoint("g5"),
            new AlgebraicPoint("g6"),
        };

        foreach (var dest in expectedDestinations)
        {
            moves[..moveCount].ToArray().Should().Contain(m => m.To == dest.AsIdx());
        }
    }
}
