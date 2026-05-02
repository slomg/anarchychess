using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.ConditionTests;

public class OwnFirstMoveIsConditionTests
{
    [Fact]
    public void Evaluate_returns_false_when_no_conditions_match()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();

        OwnFirstMoveIsCondition condition = new(new PredicateMoveCondition(_ => false));

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_true_when_all_conditions_match()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();
        var firstMove = snapshot.Board.Moves[0];

        OwnFirstMoveIsCondition condition = new(
            new PredicateMoveCondition(move => true),
            new PredicateMoveCondition(move => move == firstMove)
        );

        condition.Evaluate(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_when_any_condition_fails()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();
        var firstMove = snapshot.Board.Moves[0];

        OwnFirstMoveIsCondition condition = new(
            new PredicateMoveCondition(move => move == firstMove),
            new PredicateMoveCondition(_ => false)
        );

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_false_when_move_history_is_empty()
    {
        var snapshot = new GameQuestSnapshotFaker().RuleForMoves(totalPlies: 0).Generate();

        OwnFirstMoveIsCondition condition = new(new PredicateMoveCondition(_ => true));

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_false_when_black_and_only_one_ply_exists()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.Black)
            .RuleForMoves(totalPlies: 1)
            .Generate();

        OwnFirstMoveIsCondition condition = new(new PredicateMoveCondition(_ => true));

        condition.Evaluate(snapshot).Should().BeFalse();
    }

    [Theory]
    [InlineData(GameColor.White, 0)]
    [InlineData(GameColor.Black, 1)]
    public void Evaluate_uses_players_first_move_index(GameColor playerColor, int expectedIndex)
    {
        var snapshot = new GameQuestSnapshotFaker(playerColor)
            .RuleForMoves(totalPlies: 4)
            .Generate();

        Move? seenMove = null;
        OwnFirstMoveIsCondition condition = new(
            new PredicateMoveCondition(move =>
            {
                seenMove = move;
                return false;
            })
        );

        condition.Evaluate(snapshot);

        seenMove.Should().Be(snapshot.Board.Moves[expectedIndex]);
    }
}
