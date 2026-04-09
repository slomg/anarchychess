using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class PawnTestData : PawnLikeTestData
{
    public PawnTestData()
    {
        AddMoveTests(
            PieceType.Pawn,
            maxInitialMoveDistance: 3,
            promotesTo: GameLogicConstants.PromotablePieces
        );

        Add(
            PieceTestCase
                .From("b7", PieceFactory.White(PieceType.Pawn, hasMoved: true))
                .SkipAi()
                .WithPieceAt("b6", PieceFactory.White(PieceType.Rook))
                .ForEach(
                    ["a8", "b8", "c8", "a9", "b9", "c9", "a10", "b10", "c10"],
                    (position, testCase) =>
                        testCase.GoesTo(
                            position,
                            specialMoveType: SpecialMoveType.Throw,
                            trigger: ["b6"]
                        )
                )
                .GoesTo("b8")
        );
    }
}
