using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitTraitorRookDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(TraitorRookTestData))]
    public void BitTraitorRookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
