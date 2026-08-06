using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class KingTestData : TheoryData<PieceTestCase>
{
    public KingTestData()
    {
        var whiteKing = PieceFactory.White(PieceType.King);
        var blackKing = PieceFactory.Black(PieceType.King);

        Add(
            PieceTestCase
                .From("d4", whiteKing)
                .GoesTo("d5") // up
                .GoesTo("e5") // up-right
                .GoesTo("e4") // right
                .GoesTo("e3") // down-right
                .GoesTo("d3") // down
                .GoesTo("c3") // down-left
                .GoesTo("c4") // left
                .GoesTo("c5") // up-left
                .WithDescription("Open board from d4")
        );

        Add(
            PieceTestCase
                .From("d4", whiteKing)
                .WithPriorMoves(new MoveFaker().Generate(2))
                .WithFriendlyPieceAt("d5")
                .WithFriendlyPieceAt("e5")
                .WithFriendlyPieceAt("e4")
                .WithFriendlyPieceAt("e3")
                .WithFriendlyPieceAt("d3")
                .WithFriendlyPieceAt("c3")
                .WithFriendlyPieceAt("c4")
                .WithFriendlyPieceAt("c5")
                .WithDescription("Surrounded by friendly pieces, no moves")
        );

        Add(
            PieceTestCase
                .From("d4", whiteKing)
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("e5")
                .WithEnemyPieceAt("e4")
                .WithEnemyPieceAt("e3")
                .WithEnemyPieceAt("d3")
                .WithEnemyPieceAt("c3")
                .WithEnemyPieceAt("c4")
                .WithEnemyPieceAt("c5")
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("e5", captures: ["e5"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("e3", captures: ["e3"])
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("c3", captures: ["c3"])
                .GoesTo("c4", captures: ["c4"])
                .GoesTo("c5", captures: ["c5"])
                .WithDescription("Surrounded by enemies, all moves are captures")
        );

        Add(
            PieceTestCase
                .From("a1", whiteKing)
                .GoesTo("a2") // up
                .GoesTo("b2") // up-right
                .GoesTo("b1") // right
                .WithDescription("Edge of board, king on a1")
        );

        Add(
            PieceTestCase
                .From("j10", whiteKing)
                .GoesTo("i10") // left
                .GoesTo("i9") // down-left
                .GoesTo("j9") // down
                .WithDescription("Corner case, king on j10 (top-right corner)")
        );

        Add(
            PieceTestCase
                .From("h1", whiteKing)
                .WithFriendlyPieceAt("i2")
                .WithEnemyPieceAt("g2")
                .GoesTo("h2") // up
                // i2 blocked
                .GoesTo("i1") // right
                .GoesTo("g1") // left
                .GoesTo("g2", captures: ["g2"]) // up-left capture
                .WithDescription("King on h1, friend at i2, enemy at g2")
        );

        var whiteRook = PieceFactory.White(PieceType.Rook, hasMoved: false);
        var blackRook = PieceFactory.Black(PieceType.Rook, hasMoved: false);
        string[] whitef1Moves = ["e1", "e2", "f2", "g2", "g1"];
        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = false })
                .WithPieceAt("j1", whiteRook) // Kingside rook
                .WithPieceAt("a1", whiteRook) // Queenside rook
                .WithPieceAt("f10", whiteRook) // Vertical rook
                .GoesTo(
                    "h1",
                    trigger: ["h1", "i1"],
                    sideEffects: [new(From: new("j1"), To: new("g1"), whiteRook)],
                    specialMoveType: SpecialMoveType.KingsideCastle
                )
                .GoesTo(
                    "d1",
                    trigger: ["d1", "c1", "b1"],
                    sideEffects: [new(From: new("a1"), To: new("e1"), whiteRook)],
                    specialMoveType: SpecialMoveType.QueensideCastle
                )
                .GoesTo(
                    "f3",
                    trigger: ["f3", "f4", "f5", "f6", "f7", "f8", "f9"],
                    sideEffects: [new(From: new("f10"), To: new("f2"), whiteRook)],
                    specialMoveType: SpecialMoveType.VerticalCastle
                )
                .GoesTo(whitef1Moves)
                .WithDescription("White king on f1 with rooks in castling position")
        );

        Add(
            PieceTestCase
                .From("f10", blackKing with { HasMoved = false })
                .WithPieceAt("j10", blackRook) // Kingside rook
                .WithPieceAt("a10", blackRook) // Queenside rook
                .WithPieceAt("f1", blackRook) // Vertical rook
                .GoesTo(
                    "h10",
                    trigger: ["h10", "i10"],
                    sideEffects: [new(From: new("j10"), To: new("g10"), blackRook)],
                    specialMoveType: SpecialMoveType.KingsideCastle
                )
                .GoesTo(
                    "d10",
                    trigger: ["d10", "c10", "b10"],
                    sideEffects: [new(From: new("a10"), To: new("e10"), blackRook)],
                    specialMoveType: SpecialMoveType.QueensideCastle
                )
                .GoesTo(
                    "f8",
                    trigger: ["f2", "f3", "f4", "f5", "f6", "f7", "f8"],
                    sideEffects: [new(From: new("f1"), To: new("f9"), blackRook)],
                    specialMoveType: SpecialMoveType.VerticalCastle
                )
                // regular moves
                .GoesTo("e10")
                .GoesTo("e9")
                .GoesTo("f9")
                .GoesTo("g9")
                .GoesTo("g10")
                .WithDescription("Black king on f10 with rooks in castling position")
        );

        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = true })
                .WithPieceAt("j1", whiteRook)
                .GoesTo(whitef1Moves)
                .WithDescription("King on castling position, but has moved")
        );

        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = false })
                .WithPieceAt("j1", whiteRook with { HasMoved = true })
                .GoesTo(whitef1Moves)
                .WithDescription("King on castling position, but rook has moved")
        );

        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = false })
                .WithPieceAt("j1", whiteRook)
                .WithPieceAt("h1", PieceFactory.White(PieceType.Bishop))
                .GoesTo(
                    "h1",
                    trigger: ["h1", "i1"],
                    sideEffects: [new(From: new("j1"), To: new("g1"), whiteRook)],
                    specialMoveType: SpecialMoveType.KingsideCastle,
                    captures: ["h1"]
                )
                .GoesTo(whitef1Moves)
                .WithDescription("Self bishop capture while castling")
        );

        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = false })
                .WithPieceAt("j1", whiteRook)
                .WithPieceAt("g1", PieceFactory.White(PieceType.Bishop))
                .WithPieceAt("h1", PieceFactory.White(PieceType.Bishop))
                .GoesTo(
                    "h1",
                    trigger: ["h1", "i1"],
                    sideEffects: [new(From: new("j1"), To: new("g1"), whiteRook)],
                    specialMoveType: SpecialMoveType.KingsideCastle,
                    captures: ["g1", "h1"]
                )
                .GoesTo("e1", "e2", "f2", "g2")
                .WithDescription("Double self bishop capture while castling")
        );

        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = false })
                .WithPieceAt("j1", whiteRook)
                .WithPieceAt("a1", whiteRook)
                .WithPieceAt("f10", whiteRook)
                .WithPieceAt("h1", PieceFactory.Black(PieceType.Bishop))
                .WithPieceAt("f3", PieceFactory.Black(PieceType.Bishop))
                .WithPieceAt("d1", PieceFactory.Black(PieceType.Bishop))
                .GoesTo(whitef1Moves)
                .WithDescription("Opponent bishop blocking castling")
        );

        Add(
            PieceTestCase
                .From("f1", whiteKing with { HasMoved = false })
                .WithPieceAt("j1", whiteRook)
                .WithPieceAt("g1", PieceFactory.White(PieceType.Bishop))
                .WithFriendlyPieceAt("h1")
                .GoesTo("e1", "e2", "f2", "g2")
                .WithDescription(
                    "Bishop would be self captured if castled, but another piece is blocking"
                )
        );
        var blackQueen = PieceFactory.Black(PieceType.Queen);
        MoveTestCase LaBastarda = new(
            "e5",
            Spawns: [new PieceSpawn(PieceType.UnderagePawn, GameColor.Black, new("e5"))],
            SpecialMoveType: SpecialMoveType.LaBastarda
        );
        Add(
            PieceTestCase
                .From("e5", whiteKing)
                .WithPieceAt("f5", blackQueen)
                .GoesTo(LaBastarda with { To = "d4" })
                .GoesTo(LaBastarda with { To = "d5" })
                .GoesTo(LaBastarda with { To = "d6" })
                .GoesTo("e4", "e6", "f4", "f6")
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("King triggers LaBastarda with a single queen on left")
        );
        LaBastarda = LaBastarda with
        {
            Spawns = [new PieceSpawn(PieceType.UnderagePawn, GameColor.Black, new("a5"))],
        };
        Add(
            PieceTestCase
                .From("a5", whiteKing)
                .WithPieceAt("a6", blackQueen)
                .GoesTo(LaBastarda with { To = "a4" })
                .GoesTo(LaBastarda with { To = "b4" })
                .GoesTo("b5", "b6")
                .GoesTo("a6", captures: ["a6"])
                .WithDescription(
                    "King triggers LaBastarda with a single queen on top while near left wall"
                )
        );
        LaBastarda = LaBastarda with
        {
            Spawns = [new PieceSpawn(PieceType.UnderagePawn, GameColor.Black, new("d10"))],
        };
        Add(
            PieceTestCase
                .From("d10", whiteKing)
                .WithPieceAt("e9", blackQueen)
                .GoesTo(LaBastarda with { To = "c9" })
                .GoesTo(LaBastarda with { To = "c10" })
                .GoesTo("d9", "e10")
                .GoesTo("e9", captures: ["e9"])
                .WithDescription("King triggers LaBastarda on top wall with bottom right queen")
        );
        LaBastarda = LaBastarda with
        {
            Spawns = [new PieceSpawn(PieceType.UnderagePawn, GameColor.Black, new("f5"))],
        };
        Add(
            PieceTestCase
                .From("f5", whiteKing)
                .WithPieceAt("e5", blackQueen)
                .WithPieceAt("g4", blackQueen)
                .GoesTo(LaBastarda with { To = "e4" })
                .GoesTo(LaBastarda with { To = "e5", Captures = ["e5"] })
                .GoesTo(LaBastarda with { To = "e6" })
                .GoesTo(LaBastarda with { To = "f6" })
                .GoesTo(LaBastarda with { To = "g6" })
                .GoesTo(LaBastarda with { To = "g5" })
                .GoesTo(LaBastarda with { To = "g4", Captures = ["g4"] })
                .GoesTo("f4")
                .WithDescription("King triggers LaBastarda with a multiple queens")
        );
    }
}
