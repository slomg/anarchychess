using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class BishopDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(BishopTestData))]
    public void BishopDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
