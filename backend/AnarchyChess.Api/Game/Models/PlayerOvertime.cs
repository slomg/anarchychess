using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.NextOvertimeRemoval")]
public record NextOvertimeRemoval(
    AlgebraicPoint RemoveFrom,
    PieceType PieceType,
    GameColor? PieceColor
);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.PlayerOvertime")]
public class PlayerOvertime
{
    [Id(0)]
    public TimeSpan Remainder { get; set; }

    [Id(2)]
    public required TimeSpan RemovalInterval { get; set; }

    [Id(1)]
    public NextOvertimeRemoval? PickedNextRemoval { get; set; }
}
