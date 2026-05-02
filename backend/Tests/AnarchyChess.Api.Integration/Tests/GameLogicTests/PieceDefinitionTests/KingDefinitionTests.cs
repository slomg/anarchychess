using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class KingDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(KingTestData))]
    public void KingDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);

    [Theory]
    [ClassData(typeof(KingDefinitionTestData))]
    public void KingDefinition_evaluates_expected_specific_definition_positions(
        PieceTestCase testCase
    ) => TestMoves(testCase);
}

public class KingDefinitionTestData : TheoryData<PieceTestCase>
{
    public KingDefinitionTestData()
    {
        var unmovedWhiteKing = PieceFactory.White(PieceType.King, hasMoved: false);

        Add(
            PieceTestCase
                .From("f1", unmovedWhiteKing)
                .WithPieceAt("f2", PieceFactory.White(PieceType.Pawn))
                .WithPieceAt("e2", PieceFactory.White(PieceType.Pawn))
                .WithPieceAt("g2", PieceFactory.White(PieceType.Pawn))
                .GoesTo("e1", "g1")
                .GoesTo(
                    "f2",
                    captures: ["f2"],
                    specialMoveType: SpecialMoveType.HyperAcceleratedBongcloud
                )
                .WithDescription("Hyper accelerated bongcloud")
        );
    }
}
