using AnarchyChess.Api.GameLogic.Models;
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

        var movedPawn = PieceFactory.White(PieceType.Pawn, hasMoved: true);
        Add(
            PieceTestCase
                .From("b7", movedPawn)
                .SkipAi()
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
