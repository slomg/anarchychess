using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class MoveHistoryTests
{
    private readonly MoveHistory _history = new();

    [Fact]
    public void AddMove_adds_move_and_returns_snapshot()
    {
        MoveResult moveResult = new(
            Move: new MoveFaker().Generate(),
            MovePath: new MovePathFaker().Generate(),
            Fen: new FenNotation(Position: "some fen", FullFen: "some full fen"),
            San: "e4",
            EndStatus: null
        );

        var nextPlayer = GameColor.Black;
        var timeLeft = 123.45;

        var snapshot = _history.AddMove(nextPlayer, moveResult, timeLeft);

        _history.Moves.Should().HaveCount(1);
        snapshot.Should().Be(_history.Moves[0]);

        MoveSnapshot expectedSnapshot = new(
            moveResult.MovePath,
            moveResult.Fen.FullFen,
            nextPlayer,
            moveResult.San,
            timeLeft
        );
        snapshot.Should().BeEquivalentTo(expectedSnapshot);
    }

    [Fact]
    public void AddMoveWithOvertimeRemovals_sets_overtime_removal_indices()
    {
        MoveResult moveResult = new(
            Move: new MoveFaker().Generate(),
            MovePath: new MovePathFaker().RuleFor(x => x.OvertimeRemovalIdxs, []).Generate(),
            Fen: new FenNotation(Position: "fen", FullFen: "full fen"),
            San: "e4",
            EndStatus: null
        );
        List<AlgebraicPoint> removals = [new("a1"), new("c3")];

        var snapshot = _history.AddMoveWithOvertimeRemovals(
            GameColor.White,
            moveResult,
            timeLeft: 10,
            overtimeRemovals: removals,
            boardWidth: 10
        );

        snapshot
            .Path.OvertimeRemovalIdxs.Should()
            .BeEquivalentTo(removals.Select(r => r.AsIndex(10)));
    }

    [Fact]
    public void CommitOvertimeRemovals_does_nothing_when_no_moves_exist()
    {
        List<AlgebraicPoint> removals = [new("b2")];

        _history.CommitOvertimeRemovals(removals, boardWidth: 10);

        _history.Moves.Should().BeEmpty();
    }

    [Fact]
    public void CommitOvertimeRemovals_updates_only_last_move()
    {
        _history.AddMove(
            GameColor.White,
            new MoveResult(
                new MoveFaker().Generate(),
                new MovePathFaker().RuleFor(x => x.OvertimeRemovalIdxs, []).Generate(),
                new FenNotation("f1", "ff1"),
                "e4",
                EndStatus: null
            ),
            timeLeft: 30
        );

        _history.AddMove(
            GameColor.Black,
            new MoveResult(
                new MoveFaker().Generate(),
                new MovePathFaker().RuleFor(x => x.OvertimeRemovalIdxs, []).Generate(),
                new FenNotation("f2", "ff2"),
                "e5",
                EndStatus: null
            ),
            timeLeft: 25
        );

        List<AlgebraicPoint> removals = [new("h8"), new("a1")];

        _history.CommitOvertimeRemovals(removals, 10);

        _history.Moves[0].Path.OvertimeRemovalIdxs.Should().BeEmpty();
        _history
            .Moves[1]
            .Path.OvertimeRemovalIdxs.Should()
            .BeEquivalentTo(removals.Select(r => r.AsIndex(10)));
    }
}
