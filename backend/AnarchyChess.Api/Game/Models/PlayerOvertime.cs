using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.PlayerOvertime")]
public class PlayerOvertime
{
    [Id(0)]
    public TimeSpan Remainder { get; set; }

    [Id(1)]
    public AlgebraicPoint? PickedNextRemoval { get; set; }
}
