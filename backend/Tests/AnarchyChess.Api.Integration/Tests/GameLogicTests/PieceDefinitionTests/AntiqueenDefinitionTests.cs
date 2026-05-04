using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class AntiqueenDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(AntiqueenTestData))]
    public void AntiqueenDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);

    [Theory]
    [ClassData(typeof(AntiqueenDefinitionTestData))]
    public void AntiqueenDefinition_evaluates_expected_specific_definition_positions(
        PieceTestCase testCase
    ) => TestMoves(testCase);
}

public class AntiqueenDefinitionTestData : TheoryData<PieceTestCase>
{
    public AntiqueenDefinitionTestData()
    {
        var whiteAntiqueen = PieceFactory.White(PieceType.Antiqueen);
        var whiteQueen = PieceFactory.White(PieceType.Queen);
        Add(
            PieceTestCase
                .From("i1", whiteAntiqueen)
                .WithPieceAt("e1", whiteQueen)
                .GoesTo("j3", "h3", "g2")
                .GoesTo(
                    "e1",
                    sideEffects: [new(From: new("e1"), To: new("i1"), Piece: whiteQueen)],
                    specialMoveType: SpecialMoveType.QueentumTunnel
                )
                .WithDescription("Antiqueen queentum tunneling")
        );
    }
}
