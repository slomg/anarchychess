using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public sealed class UnderagePawnBishopCaptureQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(3, QuestDifficulty.Easy),
            CreateVariant(6, QuestDifficulty.Medium),
            CreateVariant(9, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int pawns, QuestDifficulty difficulty) =>
        new(
            Description: $"Capture {pawns} underage pawns with your bishop",
            Difficulty: difficulty,
            Target: pawns,
            Conditions: () => [],
            Progressors: () =>

                [
                    new OwnMoveCountMetric(
                        new IsMoveOfPiece(PieceType.Bishop),
                        new IsMoveCaptureOf(PieceType.UnderagePawn)
                    ),
                ]
        );
}
