using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class HorseyTestData : KnightLikeTestData
{
    public HorseyTestData()
    {
        var horsey = PieceFactory.White(PieceType.Horsey);
        AddKnightLikeMoves(horsey);

        Add(
            PieceTestCase
                .From("e5", horsey)
                .WithWhitePieceAt("d7", PieceType.Rook)
                .WithEnemyPieceAt("c8")
                .WithFriendlyPieceAt("d8", excludePieces: PieceType.Rook)
                .WithEnemyPieceAt("e8")
                .WithFriendlyPieceAt("c7", excludePieces: PieceType.Rook)
                .WithEnemyPieceAt("e7")
                .WithFriendlyPieceAt("c6", excludePieces: PieceType.Rook)
                .WithEnemyPieceAt("d6")
                .WithFriendlyPieceAt("e6", excludePieces: PieceType.Rook)
                .GoesTo(
                    "d7",
                    captures: ["d7", "c6", "d6", "e6", "c7", "e7", "c8", "d8", "e8"],
                    promotesTo: PieceType.Knook,
                    specialMoveType: SpecialMoveType.KnooklearFusion
                )
                .GoesTo("f7", "g6", "c4", "g4", "d3", "f3")
                .WithDescription("Fuses with rook and explodes surrounding pieces")
        );

        Add(
            PieceTestCase
                .From("e5", horsey)
                .WithBlackPieceAt("d7", PieceType.Rook)
                .WithEnemyPieceAt("c8")
                .WithFriendlyPieceAt("d8", excludePieces: PieceType.Rook)
                .WithEnemyPieceAt("e8")
                .WithFriendlyPieceAt("c7", excludePieces: PieceType.Rook)
                .WithEnemyPieceAt("e7")
                .WithFriendlyPieceAt("c6", excludePieces: PieceType.Rook)
                .WithEnemyPieceAt("d6")
                .WithFriendlyPieceAt("e6", excludePieces: PieceType.Rook)
                .GoesTo("d7", captures: ["d7"])
                .GoesTo("f7", "g6", "c4", "g4", "d3", "f3")
                .WithDescription("Doesn't fuses with enemy rook")
        );
    }
}
