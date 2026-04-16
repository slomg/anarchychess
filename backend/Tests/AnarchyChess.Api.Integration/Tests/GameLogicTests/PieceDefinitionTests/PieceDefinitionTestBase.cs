using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.EngineTests.Shared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public abstract class PieceDefinitionTestBase : BaseIntegrationTest
{
    private readonly ILegalMoveCalculator _legalMoveCalculator;

    public PieceDefinitionTestBase(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _legalMoveCalculator = Scope.ServiceProvider.GetRequiredService<ILegalMoveCalculator>();
    }

    protected void TestMoves(PieceTestCase testCase)
    {
        ChessBoard board = new(
            moves: testCase.PriorMoves,
            sideToMove: testCase.MovingPlayer,
            stunnedPieces: testCase.Stunned
        );
        board.PlacePiece(testCase.Origin, testCase.Piece);

        foreach (var (point, piece) in testCase.BlockedBy)
            board.PlacePiece(point, piece);

        var result = _legalMoveCalculator
            .CalculateLegalMovesForPiece(board, testCase.Origin)
            .ToList();

        var expectedMoveSorted = testCase
            .ExpectedMoves.OrderBy(m => m.To.AsIdx())
            .ThenBy(m => string.Join(",", m.IntermediateSquares.Select(i => i.Position.AsIdx())))
            .ToList();
        var resultSorted = result
            .OrderBy(m => m.To.AsIdx())
            .ThenBy(m => string.Join(",", m.IntermediateSquares.Select(i => i.Position.AsIdx())))
            .ToList();
        resultSorted.Should().BeEquivalentTo(expectedMoveSorted);
    }
}
