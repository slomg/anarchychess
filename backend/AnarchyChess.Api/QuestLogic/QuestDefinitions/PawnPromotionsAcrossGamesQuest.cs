using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class PawnPromotionsAcrossGamesQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            CreateVariant(3, QuestDifficulty.Easy),
            CreateVariant(5, QuestDifficulty.Medium),
            CreateVariant(7, QuestDifficulty.Hard),
        ];

    private static QuestVariant CreateVariant(int promotions, QuestDifficulty difficulty) =>
        new(
            Description: $"Promote a total of {promotions} pawns across multiple games",
            Difficulty: difficulty,
            Target: promotions,
            Conditions: () => [],
            Progressors: () =>

                [
                    new OwnMoveCountMetric(
                        new IsMoveOfPiece(PieceType.Pawn, PieceType.UnderagePawn),
                        new IsMovePromotion()
                    ),
                ]
        );
}
