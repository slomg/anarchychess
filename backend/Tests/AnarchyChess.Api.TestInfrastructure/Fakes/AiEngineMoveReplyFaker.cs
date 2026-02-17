using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class AiEngineMoveReplyFaker : RecordFaker<AiEngineMoveReply>
{
    public AiEngineMoveReplyFaker()
    {
        StrictMode(true);
        RuleFor(
            x => x.From,
            f => new AlgebraicPoint(X: f.Random.Number(0, 9), Y: f.Random.Number(0, 9))
        );
        RuleFor(
            x => x.To,
            f => new AlgebraicPoint(X: f.Random.Number(0, 9), Y: f.Random.Number(0, 9))
        );
        RuleFor(x => x.Captures, GameTestData.RandomPoints);
        RuleFor(x => x.PromotesTo, f => f.PickRandom<PieceType>());
    }
}
