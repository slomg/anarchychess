using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public sealed class ThrowStunQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; } =
        [
            CreateVariant(100, QuestDifficulty.Easy),
            CreateVariant(200, QuestDifficulty.Medium),
            CreateVariant(300, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int numOfPieces, QuestDifficulty difficulty) =>
        new(
            Description: $"Stun {numOfPieces} pieces by throwing pawns",
            Difficulty: difficulty,
            Target: numOfPieces,
            Conditions: () => [],
            Progressors: () =>

                [
                    new OwnMoveCountMetric(
                        new IsMoveOpponentStun(),
                        new IsMoveOfType(SpecialMoveType.Throw)
                    ),
                ]
        );
}
