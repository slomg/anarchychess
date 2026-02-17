using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitUnderagePawnDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(UnderagePawnTestData))]
    public void BitUnderagePawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
