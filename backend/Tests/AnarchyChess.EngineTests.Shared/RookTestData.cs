using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class RookTestData : TheoryData<PieceTestCase>
{
    public RookTestData()
    {
        var rook = PieceFactory.White(PieceType.Rook);

        Add(
            PieceTestCase
                .From("e5", rook)
                // vertical up
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                // vertical down
                .GoesTo("e4")
                .GoesTo("e3")
                .GoesTo("e2")
                .GoesTo("e1")
                // horizontal left
                .GoesTo("d5")
                .GoesTo("c5")
                .GoesTo("b5")
                .GoesTo("a5")
                // horizontal right
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5")
                .GoesTo("i5")
                .GoesTo("j5")
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", rook)
                // vertical up
                .GoesTo("a2")
                .GoesTo("a3")
                .GoesTo("a4")
                .GoesTo("a5")
                .GoesTo("a6")
                .GoesTo("a7")
                .GoesTo("a8")
                .GoesTo("a9")
                .GoesTo("a10")
                // horizontal right
                .GoesTo("b1")
                .GoesTo("c1")
                .GoesTo("d1")
                .GoesTo("e1")
                .GoesTo("f1")
                .GoesTo("g1")
                .GoesTo("h1")
                .GoesTo("i1")
                .GoesTo("j1")
                .WithDescription("Rook at a1")
        );

        Add(
            PieceTestCase
                .From("a5", rook)
                // vertical up
                .GoesTo("a6")
                .GoesTo("a7")
                .GoesTo("a8")
                .GoesTo("a9")
                .GoesTo("a10")
                // vertical down
                .GoesTo("a4")
                .GoesTo("a3")
                .GoesTo("a2")
                .GoesTo("a1")
                // horizontal right
                .GoesTo("b5")
                .GoesTo("c5")
                .GoesTo("d5")
                .GoesTo("e5")
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5")
                .GoesTo("i5")
                .GoesTo("j5")
                .WithDescription("Rook at a5")
        );

        Add(
            PieceTestCase
                .From("a1", rook)
                .WithFriendlyPieceAt("d1", excludePieces: [PieceType.Horsey])
                .WithEnemyPieceAt("e1")
                .WithFriendlyPieceAt("a4", excludePieces: [PieceType.Horsey])
                .WithEnemyPieceAt("a5")
                .GoesTo("a2", "a3", "b1", "c1")
                .WithDescription("Rook at a1 with blockers")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithFriendlyPieceAt("e7", excludePieces: [PieceType.Horsey]) // blocks beyond e6
                .WithFriendlyPieceAt("h5", excludePieces: [PieceType.Horsey]) // blocks beyond g5
                // vertical up
                .GoesTo("e6")
                // vertical down
                .GoesTo("e4")
                .GoesTo("e3")
                .GoesTo("e2")
                .GoesTo("e1")
                // horizontal left
                .GoesTo("d5")
                .GoesTo("c5")
                .GoesTo("b5")
                .GoesTo("a5")
                // horizontal right
                .GoesTo("f5")
                .GoesTo("g5")
                .WithDescription("Blocked by friendly piece up and right")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithEnemyPieceAt("e3") // can capture
                .WithEnemyPieceAt("b5") // can capture
                // vertical up
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                // vertical down
                .GoesTo("e4")
                .GoesTo("e3", captures: ["e3"])
                // horizontal left
                .GoesTo("d5")
                .GoesTo("c5")
                .GoesTo("b5", captures: ["b5"])
                // horizontal right
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5")
                .GoesTo("i5")
                .GoesTo("j5")
                .WithDescription("Blocked by enemy piece down and left")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithFriendlyPieceAt("e6", excludePieces: [PieceType.Horsey])
                .WithFriendlyPieceAt("e4", excludePieces: [PieceType.Horsey])
                .WithFriendlyPieceAt("d5", excludePieces: [PieceType.Horsey])
                .WithFriendlyPieceAt("f5", excludePieces: [PieceType.Horsey])
                .WithDescription("Surrounded by friendly pieces in all directions")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithEnemyPieceAt("e6")
                .WithEnemyPieceAt("e4")
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("f5")
                .GoesTo("e6", captures: ["e6"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("Surrounded by enemy pieces in all directions")
        );

        Add(
            PieceTestCase
                .From("a1", rook)
                .WithWhitePieceAt("a3", PieceType.Horsey)
                .WithEnemyPieceAt("a4")
                .WithFriendlyPieceAt("b4", excludePieces: PieceType.Horsey)
                .WithEnemyPieceAt("b3")
                .GoesTo(
                    "a3",
                    captures: ["a3", "a4", "b4", "b3"],
                    promotesTo: PieceType.Knook,
                    specialMoveType: SpecialMoveType.KnooklearFusion
                )
                .GoesTo("a2", "b1", "c1", "d1", "e1", "f1", "g1", "h1", "i1", "j1")
                .WithDescription("Fuses with horsey and explodes surrounding pieces")
        );

        Add(
            PieceTestCase
                .From("a1", rook)
                .WithBlackPieceAt("a3", PieceType.Horsey)
                .WithEnemyPieceAt("a4")
                .WithFriendlyPieceAt("b4", excludePieces: PieceType.Horsey)
                .WithEnemyPieceAt("b3")
                .GoesTo("a3", captures: ["a3"])
                .GoesTo("a2", "b1", "c1", "d1", "e1", "f1", "g1", "h1", "i1", "j1")
                .WithDescription("Doesn't fuse with enemy horsey")
        );

        Add(
            PieceTestCase
                .From("a1", rook)
                .WithWhitePieceAt("b1", PieceType.Horsey)
                .WithEnemyPieceAt("b2")
                .WithEnemyPieceAt("c2")
                .GoesTo(
                    "b1",
                    captures: ["b1", "b2", "c2"],
                    promotesTo: PieceType.Knook,
                    specialMoveType: SpecialMoveType.KnooklearFusion
                )
                .GoesTo("a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "a10")
                .WithDescription("Knooklear fusion doesn't include origin position")
        );
    }
}
