using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitRookDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(RookTestData))]
    public void BitRookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
