using AnarchyChess.Api.GameLogic;
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
    }
}
