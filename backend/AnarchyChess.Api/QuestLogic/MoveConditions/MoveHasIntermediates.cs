using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.MoveHasIntermediates")]
public sealed class MoveHasIntermediates(int atLeast) : IMoveCondition
{
    [Id(0)]
    private readonly int _atLeast = atLeast;

    public bool Evaluate(Move move) => move.IntermediateSquares.Count >= _atLeast;
}
