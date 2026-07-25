using AnarchyChess.Api.Bots.Services;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class BotBehaviorProfileFaker : RecordFaker<BotBehaviorProfile>
{
    public BotBehaviorProfileFaker()
    {
        StrictMode(true);

        RuleFor(x => x.Depth, f => f.Random.Int(min: 1, max: 32));

        RuleFor(x => x.OpeningTemperature, f => f.Random.Double(1, 100));
        RuleFor(x => x.MiddleGameTemperature, f => f.Random.Double(1, 100));
        RuleFor(x => x.EndGameTemperature, f => f.Random.Double(1, 100));

        RuleFor(x => x.TacticalThreshold, f => f.Random.Int(50, 500));
        RuleFor(x => x.BlunderThreshold, f => f.Random.Int(-500, -50));

        RuleFor(x => x.BlunderChance, f => f.Random.Double(0, 1));
        RuleFor(x => x.TacticChance, f => f.Random.Double(0, 1));
        RuleFor(x => x.TacticChancePerMoveBonus, f => f.Random.Double(0, 1));
        RuleFor(x => x.SimpleTacticChance, f => f.Random.Double(0, 1));
        RuleFor(x => x.CheckmateChance, f => f.Random.Double(0, 1));

        RuleFor(x => x.HangPenalty, f => f.Random.Int(1, 1000));
        RuleFor(x => x.OpponentHangBonus, f => f.Random.Int(1, 1000));
        RuleFor(x => x.CausesForcedMovePenalty, f => f.Random.Int(1, 1000));
        RuleFor(x => x.MultiStepMovePenalty, f => f.Random.Int(1, 1000));

        RuleFor(x => x.LosesRookCastlingRightPenalty, f => f.Random.Int(1, 1000));
        RuleFor(x => x.LosesKingCastlingRightPenalty, f => f.Random.Int(1, 1000));

        RuleFor(x => x.BackwardsPenalty, f => f.Random.Int(1, 1000));
        RuleFor(x => x.EdgePenalty, f => f.Random.Int(1, 1000));
        RuleFor(x => x.BetaDecayPenalty, f => f.Random.Int(1, 1000));

        RuleFor(x => x.NonCentralPawnPenaltyInOpening, f => f.Random.Int(1, 1000));
        RuleFor(x => x.CastleBonus, f => f.Random.Int(1, 1000));
        RuleFor(x => x.SamePiecePenalty, f => f.Random.Int(1, 1000));
        RuleFor(x => x.ThrowPenalty, f => f.Random.Int(1, 1000));

        RuleFor(
            x => x.FinalDecisionOrder,
            _ =>
                [
                    BotMoveCategory.NormalMove,
                    BotMoveCategory.Tactic,
                    BotMoveCategory.MissableBlunder,
                ]
        );

        RuleFor(x => x.ObviousMovePredicate, _ => false);
        RuleFor(x => x.MoveFilter, (Func<CandidateBotMove, bool>?)null);
    }
}
