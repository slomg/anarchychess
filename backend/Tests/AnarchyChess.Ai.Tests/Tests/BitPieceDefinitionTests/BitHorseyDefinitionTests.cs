using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitHorseyDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(HorseyTestData))]
    public void BitHorseyDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
