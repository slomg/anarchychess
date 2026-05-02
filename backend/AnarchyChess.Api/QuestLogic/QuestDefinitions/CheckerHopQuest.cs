using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class CheckerHopQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [CreateVariant(4, QuestDifficulty.Easy), CreateVariant(6, QuestDifficulty.Medium)];

    private static QuestVariant CreateVariant(int piecesToHopOver, QuestDifficulty difficulty) =>
        new(
            Description: $"Perform a checker multi-hop that jumps over at least {piecesToHopOver} pieces (captures are not required)",
            Difficulty: difficulty,
            Target: 1,
            Conditions: () =>

                [
                    new OwnMoveOccurredCondition(
                        new IsMoveOfPiece(PieceType.Checker),
                        // first hop has no intermediates
                        new MoveHasIntermediates(atLeast: piecesToHopOver - 1)
                    ),
                ]
        );
}
