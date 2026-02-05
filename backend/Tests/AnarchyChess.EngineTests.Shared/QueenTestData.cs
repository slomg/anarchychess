using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class QueenTestData : TheoryData<PieceTestCase>
{
    public QueenTestData()
    {
        var whiteQueen = PieceFactory.White(PieceType.Queen);
        var blackQueen = PieceFactory.Black(PieceType.Queen);

        string[] e5Moves =
        [
            // up
            "e6",
            "e7",
            "e8",
            "e9",
            "e10",
            // down
            "e4",
            "e3",
            "e2",
            "e1",
            // left
            "d5",
            "c5",
            "b5",
            "a5",
            // right
            "f5",
            "g5",
            "h5",
            "i5",
            "j5",
            // up left
            "d6",
            "c7",
            "b8",
            "a9",
            // up right
            "f6",
            "g7",
            "h8",
            "i9",
            "j10",
            // down left
            "d4",
            "c3",
            "b2",
            "a1",
            // down right
            "f4",
            "g3",
            "h2",
            "i1",
        ];

        Add(
            PieceTestCase
                .From("e5", whiteQueen)
                .GoesTo(e5Moves)
                // radioactive beta decay
                .GoesTo(
                    "e5",
                    spawns:
                    [
                        new PieceSpawn(PieceType.Rook, GameColor.White, new("d5")),
                        new PieceSpawn(PieceType.SterilePawn, GameColor.White, new("e6")),
                        new PieceSpawn(PieceType.Horsey, GameColor.White, new("f5")),
                    ],
                    captures: ["e5"],
                    specialMoveType: SpecialMoveType.RadioactiveBetaDecay
                )
                .WithDescription("White queen on open board from e5 with beta decay")
        );

        Add(
            PieceTestCase
                .From("e5", blackQueen)
                .GoesTo(e5Moves)
                // radioactive beta decay
                .GoesTo(
                    "e5",
                    spawns:
                    [
                        new PieceSpawn(PieceType.Rook, GameColor.White, new("d5")),
                        new PieceSpawn(PieceType.SterilePawn, GameColor.White, new("e4")),
                        new PieceSpawn(PieceType.Horsey, GameColor.White, new("f5")),
                    ],
                    captures: ["e5"],
                    specialMoveType: SpecialMoveType.RadioactiveBetaDecay
                )
                .WithDescription("Black queen on open board from e5 with beta decay")
        );

        Add(
            PieceTestCase
                .From("a1", whiteQueen)
                // vertical up
                .GoesTo("a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "a10")
                // horizontal right
                .GoesTo("b1", "c1", "d1", "e1", "f1", "g1", "h1", "i1", "j1")
                // diagonal up-right
                .GoesTo("b2", "c3", "d4", "e5", "f6", "g7", "h8", "i9", "j10")
                .WithDescription("Queen in corner a1, no beta decay")
        );

        Add(
            PieceTestCase
                .From("a5", whiteQueen)
                .WithFriendlyPieceAt("a7") // friendly above, blocks beyond a6 vertical up
                .WithEnemyPieceAt("c5") // enemy right side, can capture at c5 but no further right
                // vertical up
                .GoesTo("a6")
                // horizontal right
                .GoesTo("b5")
                .GoesTo("c5", captures: ["c5"])
                // vertical down
                .GoesTo("a4", "a3", "a2", "a1")
                // diagonal up-right
                .GoesTo("b6", "c7", "d8", "e9", "f10")
                // diagonal down-right
                .GoesTo("b4", "c3", "d2", "e1")
                .WithDescription("Queen on edge a5 with blockers, no beta decay")
        );

        Add(
            PieceTestCase
                .From("j5", whiteQueen)
                // up
                .GoesTo("j6", "j7", "j8", "j9", "j10")
                // down
                .GoesTo("j4", "j3", "j2", "j1")
                // left
                .GoesTo("i5", "h5", "g5", "f5", "e5", "d5", "c5", "b5", "a5")
                // up left
                .GoesTo("i6", "h7", "g8", "f9", "e10")
                // down left
                .GoesTo("i4", "h3", "g2", "f1")
                .WithDescription("Queen on edge j5, no beta decay")
        );

        Add(
            PieceTestCase
                .From("e5", whiteQueen)
                .WithFriendlyPieceAt("e6")
                .WithFriendlyPieceAt("e4")
                .WithFriendlyPieceAt("d5")
                .WithFriendlyPieceAt("f5")
                .WithFriendlyPieceAt("d6")
                .WithFriendlyPieceAt("f6")
                .WithFriendlyPieceAt("d4")
                .WithFriendlyPieceAt("f4")
                .WithDescription("Queen surrounded by friendly pieces - no moves")
        );

        Add(
            PieceTestCase
                .From("e5", whiteQueen)
                .WithEnemyPieceAt("e6")
                .WithEnemyPieceAt("e4")
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("f5")
                .WithEnemyPieceAt("d6")
                .WithEnemyPieceAt("f6")
                .WithEnemyPieceAt("d4")
                .WithEnemyPieceAt("f4")
                .GoesTo("e6", captures: ["e6"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .GoesTo("d6", captures: ["d6"])
                .GoesTo("f6", captures: ["f6"])
                .GoesTo("d4", captures: ["d4"])
                .GoesTo("f4", captures: ["f4"])
                .WithDescription("Queen surrounded by enemy pieces - all moves are captures")
        );
    }
}
