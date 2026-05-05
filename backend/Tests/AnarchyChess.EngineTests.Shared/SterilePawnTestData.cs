using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class SterilePawnTestData : PawnLikeTestData
{
    public SterilePawnTestData()
    {
        AddMoveTests(
            PieceType.SterilePawn,
            maxInitialMoveDistance: 1,
            promotesTo:
            [
                .. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen),
                PieceType.SterilePawn,
            ]
        );
    }
}
