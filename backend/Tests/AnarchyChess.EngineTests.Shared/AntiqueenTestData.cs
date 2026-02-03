using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public class AntiqueenTestData : KnightLikeTestData
{
    public AntiqueenTestData()
    {
        AddKnightLikeMoves(PieceFactory.White(PieceType.Antiqueen));
    }
}
