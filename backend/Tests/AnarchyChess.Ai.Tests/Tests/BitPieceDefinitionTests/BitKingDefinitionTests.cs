using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitKingDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(KingTestData))]
    public void BitKingDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
