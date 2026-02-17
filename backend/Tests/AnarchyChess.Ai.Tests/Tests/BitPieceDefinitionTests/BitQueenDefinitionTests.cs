using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitQueenDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(QueenTestData))]
    public void BitQueenDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
