using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class PawnDefinitionTests(AnarchyChessWebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(PawnTestData))]
    public void PawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);

    [Theory]
    [ClassData(typeof(PawnDefinitionTestData))]
    public void PawnDefinition_evaluates_expected_specific_definition_positions(
        PieceTestCase testCase
    ) => TestMoves(testCase);
}

public class PawnDefinitionTestData : TheoryData<PieceTestCase>
{
    public PawnDefinitionTestData()
    {
        var movedPawn = PieceFactory.White(PieceType.Pawn, hasMoved: true);
        Add(
            PieceTestCase
                .From("b7", movedPawn)
                .WithPieceAt("b6", PieceFactory.White(PieceType.Rook))
                .ForEach(
                    ["a8", "b8", "c8", "a9", "b9", "c9"],
                    (position, testCase) =>
                        testCase.GoesTo(
                            position,
                            specialMoveType: SpecialMoveType.Throw,
                            trigger: ["b6"],
                            stuns:
                            [
                                new MoveStun(
                                    Position: new AlgebraicPoint("b7"),
                                    Piece: movedPawn,
                                    StunForTurns: 2
                                ),
                            ]
                        )
                )
                .GoesTo("b8")
                .WithDescription("Pawn throw")
        );
    }
}
