using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

public class HasClockCondition : IQuestCondition
{
    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.Pool is not null && snapshot.Clocks is not null;
}
