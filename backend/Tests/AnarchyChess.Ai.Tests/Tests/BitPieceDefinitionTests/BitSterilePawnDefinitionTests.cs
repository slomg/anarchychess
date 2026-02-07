using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitSterilePawnDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(SterilePawnTestData))]
    public void BitSterilePawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
