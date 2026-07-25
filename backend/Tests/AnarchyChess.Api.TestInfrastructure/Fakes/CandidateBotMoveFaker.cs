using AnarchyChess.Api.Bots.Services;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class CandidateBotMoveFaker : StructFaker<CandidateBotMove>
{
    public CandidateBotMoveFaker(int? evalForBot = null)
    {
        StrictMode(true);

        RuleFor(x => x.MoveEval, f => new MoveEvaluationFaker(evalForBot).Generate());
        RuleFor(x => x.IsHang, false);
        RuleFor(x => x.IsCapturingHanging, false);
        RuleFor(x => x.IsRecapture, false);
        RuleFor(x => x.CausesForcedMove, false);
        RuleFor(x => x.IsMultiStep, false);
        RuleFor(x => x.PlayabilityEval, f => f.Random.Int(min: -100, max: 100));
    }
}
