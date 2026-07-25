using AnarchyChess.Ai.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MoveEvaluationFaker : RecordFaker<MoveEvaluation>
{
    public MoveEvaluationFaker(int? evalForBot = null)
    {
        StrictMode(true);
        RuleFor(x => x.Move, f => new BitMoveFaker().Generate());
        RuleFor(x => x.EvalForBot, f => evalForBot ?? f.Random.Number(min: -10000, max: 10000));
    }
}
