using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class BishopTestData : TheoryData<PieceTestCase>
{
    public BishopTestData()
    {
        var bishop = PieceFactory.White(PieceType.Bishop);
        var friendyUnderagePawn = PieceFactory.White(PieceType.UnderagePawn);
        var enemyUnderagePawn = PieceFactory.Black(PieceType.UnderagePawn);

        PieceType[] excludePieces = [PieceType.UnderagePawn];

        IntermediateSquare[] diagonalUpLeftIntermediate1 = [new(new("a9"), IsCapture: false)];
        IntermediateSquare[] diagonalUpLeftIntermediate2 =
        [
            new(new("a9"), IsCapture: false),
            new(new("b10"), IsCapture: false),
        ];
        IntermediateSquare[] diagonalUpLeftIntermediate3 =
        [
            new(new("a9"), IsCapture: false),
            new(new("b10"), IsCapture: false),
            new(new("j2"), IsCapture: false),
        ];
        IntermediateSquare[] diagonalUpLeftIntermediate4 =
        [
            new(new("a9"), IsCapture: false),
            new(new("b10"), IsCapture: false),
            new(new("j2"), IsCapture: false),
            new(new("i1"), IsCapture: false),
        ];
        IntermediateSquare[] diagonalDownRightIntermediate1 = [new(new("i1"), IsCapture: false)];
        IntermediateSquare[] diagonalDownRightIntermediate2 =
        [
            new(new("i1"), IsCapture: false),
            new(new("j2"), IsCapture: false),
        ];
        IntermediateSquare[] diagonalDownRightIntermediate3 =
        [
            new(new("i1"), IsCapture: false),
            new(new("j2"), IsCapture: false),
            new(new("b10"), IsCapture: false),
        ];
        IntermediateSquare[] diagonalDownRightIntermediate4 =
        [
            new(new("i1"), IsCapture: false),
            new(new("j2"), IsCapture: false),
            new(new("b10"), IsCapture: false),
            new(new("a9"), IsCapture: false),
        ];

        MoveTestCase[] openE5UpLeft =
        [
            new(To: "d6"),
            new(To: "c7"),
            new(To: "b8"),
            new(To: "a9"),
            new(To: "b10", Intermediates: diagonalUpLeftIntermediate1),
            new(To: "c9", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "d8", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "e7", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "f6", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "g5", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "h4", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "i3", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "j2", Intermediates: diagonalUpLeftIntermediate2),
            new(To: "i1", Intermediates: diagonalUpLeftIntermediate3),
            new(To: "h2", Intermediates: diagonalUpLeftIntermediate4),
            new(To: "g3", Intermediates: diagonalUpLeftIntermediate4),
            new(To: "f4", Intermediates: diagonalUpLeftIntermediate4),
        ];
        MoveTestCase[] openE5UpRight =
        [
            new(To: "f6"),
            new(To: "g7"),
            new(To: "h8"),
            new(To: "i9"),
            new(To: "j10"),
        ];
        MoveTestCase[] openE5DownLeft =
        [
            new(To: "d4"),
            new(To: "c3"),
            new(To: "b2"),
            new(To: "a1"),
        ];
        MoveTestCase[] openE5DownRight =
        [
            new(To: "f4"),
            new(To: "g3"),
            new(To: "h2"),
            new(To: "i1"),
            new(To: "j2", Intermediates: diagonalDownRightIntermediate1),
            new(To: "i3", Intermediates: diagonalDownRightIntermediate2),
            new(To: "h4", Intermediates: diagonalDownRightIntermediate2),
            new(To: "g5", Intermediates: diagonalDownRightIntermediate2),
            new(To: "f6", Intermediates: diagonalDownRightIntermediate2),
            new(To: "e7", Intermediates: diagonalDownRightIntermediate2),
            new(To: "d8", Intermediates: diagonalDownRightIntermediate2),
            new(To: "c9", Intermediates: diagonalDownRightIntermediate2),
            new(To: "b10", Intermediates: diagonalDownRightIntermediate2),
            new(To: "a9", Intermediates: diagonalDownRightIntermediate3),
            new(To: "b8", Intermediates: diagonalDownRightIntermediate4),
            new(To: "c7", Intermediates: diagonalDownRightIntermediate4),
            new(To: "d6", Intermediates: diagonalDownRightIntermediate4),
        ];

        MoveTestCase[] openE5Moves =
        [
            .. openE5UpLeft,
            .. openE5UpRight,
            .. openE5DownLeft,
            .. openE5DownRight,
        ];

        Add(
            PieceTestCase
                .From("e5", bishop)
                .GoesTo(openE5Moves)
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithFriendlyPieceAt("a9", excludePieces)
                .GoesTo("d6", "c7", "b8")
                .GoesTo(openE5UpRight)
                .GoesTo(openE5DownLeft)
                .GoesTo(openE5DownRight.Where(x => x.To != "a9").ToArray())
                .WithDescription("Friendly piece blocking bounce")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithEnemyPieceAt("a9", excludePieces)
                .GoesTo("d6", "c7", "b8")
                .GoesTo("a9", captures: ["a9"])
                .GoesTo(openE5UpRight)
                .GoesTo(openE5DownLeft)
                .GoesTo(openE5DownRight.Where(x => x.To != "a9").ToArray())
                .WithDescription("Friendly piece blocking bounce")
        );

        Add(
            PieceTestCase
                .From("a1", bishop)
                // diagonal up-right only
                .GoesTo("b2")
                .GoesTo("c3")
                .GoesTo("d4")
                .GoesTo("e5")
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
                .WithDescription("Corner case: bishop at a1")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithFriendlyPieceAt("g7", excludePieces) // blocks beyond f6
                .GoesTo(openE5UpLeft)
                // diagonal up-right, stops before g7
                .GoesTo("f6")
                // cannot go to g7 (blocked by friendly)
                .GoesTo(openE5DownLeft)
                .GoesTo(openE5DownRight)
                .WithDescription("Blocked by friendly piece")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithEnemyPieceAt("c3", excludePieces) // enemy can be captured, blocks beyond
                .WithFriendlyPieceAt("b2") // friendly beyond enemy
                .GoesTo(openE5UpLeft)
                .GoesTo(openE5UpRight)
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3", captures: ["c3"])
                // cannot go beyond c3 because of enemy blocker
                .GoesTo(openE5DownRight)
                .WithDescription("Blocked by enemy piece")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithFriendlyPieceAt("d6", excludePieces)
                .WithFriendlyPieceAt("f6", excludePieces)
                .WithFriendlyPieceAt("d4", excludePieces)
                .WithFriendlyPieceAt("f4", excludePieces)
                .WithDescription("Bishop surrounded by friendly pieces on all diagonals (no moves)")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithEnemyPieceAt("d6", excludePieces)
                .WithEnemyPieceAt("f6", excludePieces)
                .WithEnemyPieceAt("d4", excludePieces)
                .WithEnemyPieceAt("f4", excludePieces)
                .GoesTo("d6", captures: ["d6"])
                .GoesTo("f6", captures: ["f6"])
                .GoesTo("d4", captures: ["d4"])
                .GoesTo("f4", captures: ["f4"])
                .WithDescription(
                    "Bishop surrounded by enemy pieces on all diagonals (can capture all"
                )
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("c7", friendyUnderagePawn)
                .WithPieceAt("h8", friendyUnderagePawn)
                .WithPieceAt("a1", friendyUnderagePawn)
                .WithPieceAt("f4", friendyUnderagePawn)
                // diagonal up-left
                .GoesTo("d6")
                .GoesTo("c7", captures: ["c7"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal up-right
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8", captures: ["h8"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3")
                .GoesTo("b2")
                .GoesTo("a1", captures: ["a1"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-right
                .GoesTo("f4", captures: ["f4"], forcedPriority: ForcedMovePriority.UnderagePawn)
                .WithDescription("Forced friendly underage pawn capture")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("a9", enemyUnderagePawn)
                .WithPieceAt("f6", enemyUnderagePawn)
                .WithPieceAt("b2", enemyUnderagePawn)
                .WithPieceAt("g3", enemyUnderagePawn)
                // diagonal up-left
                .GoesTo("d6")
                .GoesTo("c7")
                .GoesTo("b8")
                .GoesTo("a9", captures: ["a9"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal up-right
                .GoesTo("f6", captures: ["f6"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3")
                .GoesTo("b2", captures: ["b2"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-right
                .GoesTo("f4")
                .GoesTo("g3", captures: ["g3"], forcedPriority: ForcedMovePriority.UnderagePawn)
                .WithDescription("Forced enemy underage pawn capture")
        );

        var partnerIlVaticano = PieceFactory.White(PieceType.Bishop);
        var regularEnemy = PieceFactory.Black(PieceType.Rook);
        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("h5", partnerIlVaticano)
                .WithPieceAt("f5", regularEnemy)
                .WithPieceAt("g5", regularEnemy)
                // I don't wanna deal with bounces here :)
                .WithFriendlyPieceAt("d6", excludePieces)
                .WithFriendlyPieceAt("f6", excludePieces)
                .WithFriendlyPieceAt("d4", excludePieces)
                .WithFriendlyPieceAt("f4", excludePieces)
                .GoesTo(
                    "h5",
                    trigger: ["f5", "g5"],
                    captures: ["f5", "g5"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("h5"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano right moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("b5", partnerIlVaticano)
                .WithPieceAt("c5", regularEnemy)
                .WithPieceAt("d5", regularEnemy)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "b5",
                    trigger: ["c5", "d5"],
                    captures: ["c5", "d5"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("b5"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano left moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("e8", partnerIlVaticano)
                .WithPieceAt("e7", regularEnemy)
                .WithPieceAt("e6", regularEnemy)
                .WithFriendlyPieceAt("d6", excludePieces)
                .WithFriendlyPieceAt("f6", excludePieces)
                .WithFriendlyPieceAt("d4", excludePieces)
                .WithFriendlyPieceAt("f4", excludePieces)
                .GoesTo(
                    "e8",
                    trigger: ["e6", "e7"],
                    captures: ["e6", "e7"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("e8"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano up moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("e2", partnerIlVaticano)
                .WithPieceAt("e3", regularEnemy)
                .WithPieceAt("e4", regularEnemy)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "e2",
                    trigger: ["e4", "e3"],
                    captures: ["e4", "e3"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("e2"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano down moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("h5", partnerIlVaticano)
                .WithPieceAt("f5", regularEnemy)
                .WithPieceAt("g5", enemyUnderagePawn)
                .WithFriendlyPieceAt("d6", excludePieces)
                .WithFriendlyPieceAt("f6", excludePieces)
                .WithFriendlyPieceAt("d4", excludePieces)
                .WithFriendlyPieceAt("f4", excludePieces)
                .GoesTo(
                    "h5",
                    trigger: ["f5", "g5"],
                    captures: ["f5", "g5"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("h5"), To: new("e5"), partnerIlVaticano),
                    ],
                    forcedPriority: ForcedMovePriority.UnderagePawn,
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Forced il vaticano with underage pawn")
        );
    }
}
