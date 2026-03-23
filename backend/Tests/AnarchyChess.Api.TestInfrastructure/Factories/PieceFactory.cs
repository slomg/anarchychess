using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Factories;

public static class PieceFactory
{
    public static Piece White(
        PieceType? type = null,
        bool? hasMoved = null,
        int stunnedForMoves = 0
    ) => new PieceFaker(GameColor.White, type, hasMoved, stunnedForMoves).Generate();

    public static Piece Black(
        PieceType? type = null,
        bool? hasMoved = null,
        int stunnedForMoves = 0
    ) => new PieceFaker(GameColor.Black, type, hasMoved, stunnedForMoves).Generate();

    public static Piece Neutral(
        PieceType? type = null,
        bool? hasMoved = null,
        int stunnedForMoves = 0
    ) => new PieceFaker(color: null, type, hasMoved, stunnedForMoves).Generate();
}
