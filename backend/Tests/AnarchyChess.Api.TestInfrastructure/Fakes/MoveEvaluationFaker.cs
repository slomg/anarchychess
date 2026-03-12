using AnarchyChess.Ai.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MoveEvaluationFaker : StructFaker<MoveEvaluation>
{
    public MoveEvaluationFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Move, f => new BitMoveFaker().Generate());
        RuleFor(x => x.EvalForBot, f => f.Random.Number(min: -10000, max: 10000));
    }
}
