using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMoveOpponentStunTests
{
    [Fact]
    public void Evaluate_returns_true_when_stuns_opponent_piece()
    {
        var move = new MoveFaker(forColor: GameColor.White)
            .RuleFor(
                x => x.Stuns,
                [
                    new MoveStun(
                        Position: new AlgebraicPoint("a1"),
                        Piece: PieceFactory.Black(),
                        StunForTurns: 5
                    ),
                ]
            )
            .Generate();

        new IsMoveOpponentStun().Evaluate(move).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_when_stuns_only_friendly_piece()
    {
        var move = new MoveFaker(forColor: GameColor.White)
            .RuleFor(
                x => x.Stuns,
                [
                    new MoveStun(
                        Position: new AlgebraicPoint("a1"),
                        Piece: PieceFactory.White(),
                        StunForTurns: 5
                    ),
                ]
            )
            .Generate();

        new IsMoveOpponentStun().Evaluate(move).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_false_when_no_stuns_exist()
    {
        var move = new MoveFaker(forColor: GameColor.White).RuleFor(x => x.Stuns, []).Generate();

        new IsMoveOpponentStun().Evaluate(move).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_returns_true_when_any_stun_is_opponent_among_many()
    {
        var move = new MoveFaker(forColor: GameColor.White)
            .RuleFor(
                x => x.Stuns,
                [
                    new MoveStun(
                        Position: new AlgebraicPoint("a1"),
                        Piece: PieceFactory.White(),
                        StunForTurns: 5
                    ),
                    new MoveStun(
                        Position: new AlgebraicPoint("b1"),
                        Piece: PieceFactory.Black(),
                        StunForTurns: 5
                    ),
                ]
            )
            .Generate();

        new IsMoveOpponentStun().Evaluate(move).Should().BeTrue();
    }
}
