using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.BoardPieceCountMetric")]
public sealed class OwnBoardPieceCountMetric(PieceType type) : IQuestMetric
{
    [Id(0)]
    private readonly PieceType _type = type;

    public int Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.Board.GetAllPiecesWith(_type, snapshot.PlayerColor).Count;
}
