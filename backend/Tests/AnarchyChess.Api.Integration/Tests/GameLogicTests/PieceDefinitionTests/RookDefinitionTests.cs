using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class RookDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(RookTestData))]
    public void RookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
