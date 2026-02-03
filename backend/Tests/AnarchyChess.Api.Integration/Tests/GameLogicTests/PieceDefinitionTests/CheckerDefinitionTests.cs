using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class CheckerDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(CheckerTestData))]
    public void CheckerDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
