using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class MoveHasIntermediatesTests
{
    [Fact]
    public void Evaluate_returns_true_for_enough_intermediates()
    {
        var move = new MoveFaker().RuleFor(
            x => x.IntermediateSquares,
            [
                new(new("a1"), IsCapture: false),
                new(new("a2"), IsCapture: true),
                new(new("a3"), IsCapture: false),
            ]
        );
        new MoveHasIntermediates(atLeast: 3).Evaluate(move).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_true_for_not_enough_captures()
    {
        var move = new MoveFaker().RuleFor(
            x => x.IntermediateSquares,
            [new(new("a1"), IsCapture: false), new(new("a2"), IsCapture: true)]
        );
        new MoveHasIntermediates(atLeast: 3).Evaluate(move).Should().BeFalse();
    }
}
