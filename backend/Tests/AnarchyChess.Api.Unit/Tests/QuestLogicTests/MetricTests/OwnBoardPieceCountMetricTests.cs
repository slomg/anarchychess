using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class OwnBoardPieceCountMetricTests
{
    [Theory]
    [InlineData(GameColor.White, 3)]
    [InlineData(GameColor.Black, 1)]
    public void Evaluate_only_counts_your_pieces(GameColor color, int amount)
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("a1")] = PieceFactory.White(PieceType.Rook),
                [new("b1")] = PieceFactory.White(PieceType.Rook),
                [new("c1")] = PieceFactory.White(PieceType.Rook),
                [new("d1")] = PieceFactory.White(PieceType.Queen),
                [new("e1")] = PieceFactory.White(PieceType.King),

                [new("a10")] = PieceFactory.Black(PieceType.Rook),
                [new("a10")] = PieceFactory.Black(PieceType.Rook),
                [new("a10")] = PieceFactory.Black(PieceType.Rook),
                [new("a10")] = PieceFactory.Black(PieceType.Rook),
                [new("d10")] = PieceFactory.Black(PieceType.Queen),
                [new("e10")] = PieceFactory.Black(PieceType.King),
            }
        );
        var snapshot = new GameQuestSnapshotFaker(color).RuleFor(x => x.Board, board).Generate();

        new OwnBoardPieceCountMetric(PieceType.Rook).Evaluate(snapshot).Should().Be(amount);
    }
}
