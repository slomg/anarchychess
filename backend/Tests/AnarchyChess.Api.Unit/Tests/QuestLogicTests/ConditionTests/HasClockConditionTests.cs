using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class HasClockConditionTests
{
    private readonly HasClockCondition _condition = new();

    [Fact]
    public void Evaluate_returns_false_if_there_is_no_pool()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleFor(x => x.Pool, (PoolKey?)null).Generate();
        _condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_false_if_there_are_no_clocks()
    {
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.Clocks, (ClockSnapshot?)null)
            .Generate();
        _condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_true_if_there_is_a_pool_and_clocks()
    {
        var snapshot = new GameQuestSnapshotFaker().Generate();
        _condition.Evaluate(snapshot).Should().BeTrue();
    }
}
