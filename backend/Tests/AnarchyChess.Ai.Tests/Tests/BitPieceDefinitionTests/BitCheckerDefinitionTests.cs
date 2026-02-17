using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitCheckerDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(CheckerTestData))]
    public void BitCheckerDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
