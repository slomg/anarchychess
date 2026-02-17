using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitAntiqueenDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(AntiqueenTestData))]
    public void BitAntiqueenDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
