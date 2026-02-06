using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitKnookDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(KnookTestData))]
    public void BitKnookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
