using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.BotTests;

public class MoveClassifierTests
{
    private readonly MoveClassifier _classifier = new();

    [Fact]
    public void Classify_orders_mate_in_one_moves()
    {
        var behavior = new BotBehaviorProfileFaker().RuleFor(x => x.Depth, 3).Generate();

        var mate = new CandidateBotMoveFaker(evalForBot: 100_003).Generate();
        var normal = new CandidateBotMoveFaker(evalForBot: 0).Generate();

        var result = _classifier.Classify([mate, normal], lastEval: 0, behavior);

        result.MatesInOnes.Should().ContainSingle().Which.Should().Be(mate);
        result.NormalMoves.Should().ContainSingle().Which.Should().Be(normal);
    }

    [Fact]
    public void Classify_orders_missable_checkmate_moves()
    {
        var behavior = new BotBehaviorProfileFaker().Generate();
        var checkmate = new CandidateBotMoveFaker(evalForBot: 100_000)
            .RuleFor(x => x.CausesForcedMove, true)
            .Generate();

        var result = _classifier.Classify([checkmate], lastEval: 0, behavior);

        result.MissableCheckmates.Should().ContainSingle().Which.Should().Be(checkmate);
    }

    [Fact]
    public void Classify_does_not_order_non_missable_checkmates_as_missable()
    {
        var behavior = new BotBehaviorProfileFaker().Generate();
        var checkmate = new CandidateBotMoveFaker(evalForBot: 100_000)
            .RuleFor(x => x.IsCapturingHanging, true)
            .RuleFor(x => x.IsRecapture, true)
            .Generate();

        var result = _classifier.Classify([checkmate], lastEval: 0, behavior);

        result.MissableCheckmates.Should().BeEmpty();
        result.NormalMoves.Should().ContainSingle().Which.Should().Be(checkmate);
    }

    [Fact]
    public void Classify_orders_tactic_moves()
    {
        var behavior = new BotBehaviorProfileFaker()
            .RuleFor(x => x.TacticalThreshold, 100)
            .Generate();

        var tactic = new CandidateBotMoveFaker(evalForBot: 200).Generate();

        var result = _classifier.Classify([tactic], lastEval: 0, behavior);

        result.Tactics.Should().ContainSingle().Which.Should().Be(tactic);
    }

    [Fact]
    public void Classify_does_not_order_hanging_captures_as_tactics()
    {
        var behavior = new BotBehaviorProfileFaker()
            .RuleFor(x => x.TacticalThreshold, 100)
            .Generate();

        var move = new CandidateBotMoveFaker(evalForBot: 200)
            .RuleFor(x => x.IsCapturingHanging, true)
            .Generate();

        var result = _classifier.Classify([move], lastEval: 0, behavior);

        result.Tactics.Should().BeEmpty();
        result.NormalMoves.Should().ContainSingle().Which.Should().Be(move);
    }

    [Fact]
    public void Classify_orders_missable_blunder_moves()
    {
        var behavior = new BotBehaviorProfileFaker()
            .RuleFor(x => x.BlunderThreshold, -100)
            .Generate();

        var blunder = new CandidateBotMoveFaker(evalForBot: -200)
            .RuleFor(x => x.IsHang, false)
            .Generate();

        var result = _classifier.Classify([blunder], lastEval: 0, behavior);

        result.MissableBlunders.Should().ContainSingle().Which.Should().Be(blunder);
    }

    [Fact]
    public void Classify_does_not_order_hanging_moves_as_missable_blunders()
    {
        var behavior = new BotBehaviorProfileFaker()
            .RuleFor(x => x.BlunderThreshold, -100)
            .Generate();

        var hanging = new CandidateBotMoveFaker(evalForBot: -200)
            .RuleFor(x => x.IsHang, true)
            .Generate();

        var result = _classifier.Classify([hanging], lastEval: 0, behavior);

        result.MissableBlunders.Should().BeEmpty();
    }

    [Fact]
    public void Classify_orders_obvious_moves()
    {
        var behavior = new BotBehaviorProfileFaker()
            .RuleFor(x => x.ObviousMovePredicate, _ => true)
            .Generate();

        var obvious = new CandidateBotMoveFaker(evalForBot: 0).Generate();

        var result = _classifier.Classify([obvious], lastEval: 0, behavior);

        result.ObviousMoves.Should().ContainSingle().Which.Should().Be(obvious);
    }

    [Fact]
    public void Classify_orders_remaining_moves_as_normal_moves()
    {
        var behavior = new BotBehaviorProfileFaker()
            .RuleFor(x => x.ObviousMovePredicate, _ => false)
            .Generate();

        var normal = new CandidateBotMoveFaker(evalForBot: 0).Generate();

        var result = _classifier.Classify([normal], lastEval: 0, behavior);

        result.NormalMoves.Should().ContainSingle().Which.Should().Be(normal);
    }
}
