using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public sealed class WinWithHyperAcceleratedBongcloud : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(1, QuestDifficulty.Easy),
            CreateVariant(2, QuestDifficulty.Medium),
            CreateVariant(3, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfGames, QuestDifficulty difficulty) =>
        new(
            Description: $"Win {numOfGames} games after playing Hyper Accelerated Bongcloud",
            Difficulty: difficulty,
            Target: numOfGames,
            Conditions: () =>

                [
                    new WinCondition(),
                    new OwnFirstMoveIsCondition(
                        new IsMoveOfType(SpecialMoveType.HyperAcceleratedBongcloud)
                    ),
                ]
        );
}
