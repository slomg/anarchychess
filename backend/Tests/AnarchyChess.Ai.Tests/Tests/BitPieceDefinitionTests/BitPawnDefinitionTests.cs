using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitPawnDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(PawnTestData))]
    public void BitPawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
