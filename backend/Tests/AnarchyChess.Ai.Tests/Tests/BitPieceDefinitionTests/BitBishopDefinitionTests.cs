using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitBishopDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(BishopTestData))]
    public void BitBishopDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}
