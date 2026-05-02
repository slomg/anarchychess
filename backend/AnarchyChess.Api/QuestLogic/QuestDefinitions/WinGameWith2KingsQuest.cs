using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class WinGameWith2KingsQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                Description: "Finish a game with at least 2 kings of your own color on the board (promote your checker to a king)",
                Difficulty: QuestDifficulty.Medium,
                Target: 1,
                Conditions: () =>
                    [new GreaterThanEqualCondition(new OwnBoardPieceCountMetric(PieceType.King), 2)]
            ),
        ];
}
