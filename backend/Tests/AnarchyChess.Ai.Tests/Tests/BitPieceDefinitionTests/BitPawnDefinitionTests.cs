using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineTests.Shared;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitPawnDefinitionTests : BitPieceDefinitionTestBase
{
    [Theory]
    [ClassData(typeof(PawnTestData))]
    public void BitPawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);

    [Theory]
    [ClassData(typeof(BitPawnDefinitionTestData))]
    public void BitPawnDefinition_evaluates_expected_specific_bit_definition_positions(
        PieceTestCase testCase
    ) => TestMoves(testCase);
}

public class BitPawnDefinitionTestData : TheoryData<PieceTestCase>
{
    public BitPawnDefinitionTestData()
    {
        var whitePawn = PieceFactory.White(PieceType.Pawn, hasMoved: true);
        var whiteThrower = PieceFactory.White(PieceType.Queen);
        var blackTarget = PieceFactory.Black(PieceType.Rook);

        var blackPawn = PieceFactory.Black(PieceType.Pawn, hasMoved: true);
        var blackThrower = PieceFactory.Black(PieceType.Queen);
        var whiteTarget = PieceFactory.White(PieceType.Rook);

        Add(
            PieceTestCase
                .From("f2", whitePawn)
                .WithPieceAt("f1", whiteThrower)
                .WithPieceAt("e6", blackTarget)
                .WithPieceAt("f7", blackTarget)
                .WithPieceAt("g8", blackTarget)
                // can't hit
                .WithPieceAt("d5", blackTarget)
                .WithPieceAt("h9", blackTarget)
                .WithPieceAt("f10", blackTarget)
                .GoesTo("f3")
                .GoesTo("e4", specialMoveType: SpecialMoveType.Throw) // attack d5
                .GoesTo("f5", specialMoveType: SpecialMoveType.Throw) // attack e6
                .GoesTo("g6", specialMoveType: SpecialMoveType.Throw) // attack f7
                .GoesTo("e9", specialMoveType: SpecialMoveType.Throw) // attack f10
                .GoesTo("g9", specialMoveType: SpecialMoveType.Throw) // attack f10
                .GoesTo("e6", specialMoveType: SpecialMoveType.Throw, captures: ["f2"])
                .GoesTo("f7", specialMoveType: SpecialMoveType.Throw, captures: ["f2"])
                .GoesTo("g8", specialMoveType: SpecialMoveType.Throw, captures: ["f2"])
                .WithDescription("White throw forward")
        );

        Add(
            PieceTestCase
                .From("i2", whitePawn)
                .WithPieceAt("j1", whiteThrower)
                .WithPieceAt("c7", blackTarget)
                .WithPieceAt("e6", blackTarget)
                .WithPieceAt("g5", blackTarget)
                // can't hit
                .WithPieceAt("b10", blackTarget)
                .WithPieceAt("b7", blackTarget)
                .WithPieceAt("e8", blackTarget)
                .GoesTo("i3")
                .GoesTo("f4", specialMoveType: SpecialMoveType.Throw) // attack g5
                .GoesTo("h4", specialMoveType: SpecialMoveType.Throw) // attack g5
                .GoesTo("f5", specialMoveType: SpecialMoveType.Throw) // attack e6
                .GoesTo("d6", specialMoveType: SpecialMoveType.Throw) // attack c7
                .GoesTo("d7", specialMoveType: SpecialMoveType.Throw) // attack e8
                .GoesTo("a9", specialMoveType: SpecialMoveType.Throw) // attack b10
                .GoesTo("c9", specialMoveType: SpecialMoveType.Throw) // attack b10
                .GoesTo("g5", specialMoveType: SpecialMoveType.Throw, captures: ["i2"])
                .GoesTo("e6", specialMoveType: SpecialMoveType.Throw, captures: ["i2"])
                .GoesTo("c7", specialMoveType: SpecialMoveType.Throw, captures: ["i2"])
                .WithDescription("White throw left")
        );

        Add(
            PieceTestCase
                .From("b2", whitePawn)
                .WithPieceAt("a1", whiteThrower)
                .WithPieceAt("h7", blackTarget)
                .WithPieceAt("f6", blackTarget)
                .WithPieceAt("d5", blackTarget)
                // can't hit
                .WithPieceAt("i10", blackTarget)
                .WithPieceAt("i7", blackTarget)
                .WithPieceAt("f8", blackTarget)
                .GoesTo("b3")
                .GoesTo("c4", specialMoveType: SpecialMoveType.Throw) // attack d5
                .GoesTo("e4", specialMoveType: SpecialMoveType.Throw) // attack d5
                .GoesTo("e5", specialMoveType: SpecialMoveType.Throw) // attack f6
                .GoesTo("g6", specialMoveType: SpecialMoveType.Throw) // attack h7
                .GoesTo("g7", specialMoveType: SpecialMoveType.Throw) // attack f8
                .GoesTo("h9", specialMoveType: SpecialMoveType.Throw) // attack i10
                .GoesTo("j9", specialMoveType: SpecialMoveType.Throw) // attack i10
                .GoesTo("d5", specialMoveType: SpecialMoveType.Throw, captures: ["b2"])
                .GoesTo("f6", specialMoveType: SpecialMoveType.Throw, captures: ["b2"])
                .GoesTo("h7", specialMoveType: SpecialMoveType.Throw, captures: ["b2"])
                .WithDescription("White throw right")
        );

        Add(
            PieceTestCase
                .From("f9", blackPawn)
                .WithPieceAt("f10", blackThrower)
                .WithPieceAt("e5", whiteTarget)
                .WithPieceAt("f4", whiteTarget)
                .WithPieceAt("g3", whiteTarget)
                // can't hit
                .WithPieceAt("d7", whiteTarget)
                .WithPieceAt("h2", whiteTarget)
                .WithPieceAt("f1", whiteTarget)
                .GoesTo("f8")
                .GoesTo("f6", specialMoveType: SpecialMoveType.Throw) // attack e5
                .GoesTo("g5", specialMoveType: SpecialMoveType.Throw) // attack f4
                .GoesTo("e8", specialMoveType: SpecialMoveType.Throw) // attack d7
                .GoesTo("e2", specialMoveType: SpecialMoveType.Throw) // attack f1
                .GoesTo("g2", specialMoveType: SpecialMoveType.Throw) // attack f1
                .GoesTo("e5", specialMoveType: SpecialMoveType.Throw, captures: ["f9"])
                .GoesTo("f4", specialMoveType: SpecialMoveType.Throw, captures: ["f9"])
                .GoesTo("g3", specialMoveType: SpecialMoveType.Throw, captures: ["f9"])
                .WithDescription("Black throw forward")
        );

        Add(
            PieceTestCase
                .From("i9", blackPawn)
                .WithPieceAt("j10", blackThrower)
                .WithPieceAt("c4", whiteTarget)
                .WithPieceAt("e5", whiteTarget)
                .WithPieceAt("g6", whiteTarget)
                // can't hit
                .WithPieceAt("b1", whiteTarget)
                .WithPieceAt("b7", whiteTarget)
                .WithPieceAt("e3", whiteTarget)
                .GoesTo("i8")
                .GoesTo("f7", specialMoveType: SpecialMoveType.Throw) // attack g6
                .GoesTo("h7", specialMoveType: SpecialMoveType.Throw) // attack g6
                .GoesTo("f6", specialMoveType: SpecialMoveType.Throw) // attack e5
                .GoesTo("d5", specialMoveType: SpecialMoveType.Throw) // attack c4
                .GoesTo("d4", specialMoveType: SpecialMoveType.Throw) // attack e3
                .GoesTo("a2", specialMoveType: SpecialMoveType.Throw) // attack b1
                .GoesTo("c2", specialMoveType: SpecialMoveType.Throw) // attack b1
                .GoesTo("c4", specialMoveType: SpecialMoveType.Throw, captures: ["i9"])
                .GoesTo("e5", specialMoveType: SpecialMoveType.Throw, captures: ["i9"])
                .GoesTo("g6", specialMoveType: SpecialMoveType.Throw, captures: ["i9"])
                .WithDescription("Black throw left")
        );

        Add(
            PieceTestCase
                .From("b9", blackPawn)
                .WithPieceAt("a10", blackThrower)
                .WithPieceAt("h4", whiteTarget)
                .WithPieceAt("f5", whiteTarget)
                .WithPieceAt("d6", whiteTarget)
                // can't hit
                .WithPieceAt("i1", whiteTarget)
                .WithPieceAt("i7", whiteTarget)
                .WithPieceAt("f3", whiteTarget)
                .GoesTo("b8")
                .GoesTo("c7", specialMoveType: SpecialMoveType.Throw) // attack d6
                .GoesTo("e7", specialMoveType: SpecialMoveType.Throw) // attack d6
                .GoesTo("e6", specialMoveType: SpecialMoveType.Throw) // attack f5
                .GoesTo("g5", specialMoveType: SpecialMoveType.Throw) // attack h4
                .GoesTo("g4", specialMoveType: SpecialMoveType.Throw) // attack f3
                .GoesTo("h2", specialMoveType: SpecialMoveType.Throw) // attack i1
                .GoesTo("j2", specialMoveType: SpecialMoveType.Throw) // attack i1
                .GoesTo("h4", specialMoveType: SpecialMoveType.Throw, captures: ["b9"])
                .GoesTo("f5", specialMoveType: SpecialMoveType.Throw, captures: ["b9"])
                .GoesTo("d6", specialMoveType: SpecialMoveType.Throw, captures: ["b9"])
                .WithDescription("Black throw right")
        );

        Add(
            PieceTestCase
                .From("f2", whitePawn)
                .WithPieceAt("f1", whiteThrower)
                .WithPieceAt("f6", whiteTarget)
                .GoesTo("f3")
                .WithDescription("White throws can't stun friendly")
        );

        Add(
            PieceTestCase
                .From("f9", blackPawn)
                .WithPieceAt("f10", blackThrower)
                .WithPieceAt("f5", blackTarget)
                .GoesTo("f8")
                .WithDescription("Black throws can't stun friendly")
        );

        Add(
            PieceTestCase
                .From("f2", whitePawn)
                .WithMaxDepth(8)
                .WithDepth(4)
                .WithPieceAt("f1", whiteThrower)
                .WithPieceAt("f6", blackTarget)
                .GoesTo("f3")
                .WithDescription("Throws aren't generated beyond 4 depth deep")
        );

        Add(
            PieceTestCase
                .From("f2", whitePawn)
                .WithPieceAt("e1", whiteThrower)
                .WithPieceAt("f1", whiteThrower)
                .WithPieceAt("g1", whiteThrower)
                .WithStun("e1")
                .WithStun("f1")
                .WithStun("g1")
                .WithPieceAt("f6", blackTarget)
                .GoesTo("f3")
                .WithDescription("White stunned pieces can't be used to throw")
        );

        Add(
            PieceTestCase
                .From("f9", blackPawn)
                .WithPieceAt("e10", blackThrower)
                .WithPieceAt("f10", blackThrower)
                .WithPieceAt("g10", blackThrower)
                .WithStun("e10")
                .WithStun("f10")
                .WithStun("g10")
                .WithPieceAt("f6", whiteTarget)
                .GoesTo("f8")
                .WithDescription("Black stunned pieces can't be used to throw")
        );
    }
}
