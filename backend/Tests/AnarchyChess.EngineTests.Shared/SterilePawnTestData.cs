using AnarchyChess.Api.GameLogic;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class SterilePawnTestData : PawnLikeTestData
{
    private static readonly IReadOnlyCollection<PieceType> _promotesTo =
    [
        .. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen),
    ];

    public SterilePawnTestData()
    {
        AddMoveTests(PieceType.SterilePawn, maxInitialMoveDistance: 1, promotesTo: _promotesTo);
    }
}
