using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class UnderagePawnTestData : PawnLikeTestData
{
    public UnderagePawnTestData()
    {
        AddMoveTests(
            PieceType.UnderagePawn,
            maxInitialMoveDistance: 2,
            promotesTo: [.. GameLogicConstants.PromotablePieces, PieceType.UnderagePawn]
        );
    }
}
