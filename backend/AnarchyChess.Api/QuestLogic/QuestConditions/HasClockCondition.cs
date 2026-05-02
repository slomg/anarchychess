using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.HasClockCondition")]
public class HasClockCondition : IQuestCondition
{
    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.Pool is not null && snapshot.Clocks is not null;
}
