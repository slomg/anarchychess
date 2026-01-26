using AnarchyChess.Api.Game.Services;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.PlayerOvertime")]
public class PlayerOvertime
{
    [Id(0)]
    public long RemainderMs { get; set; }

    [Id(1)]
    public IReadOnlyList<PendingRemovalEntry>? PendingRemoval { get; set; }
}
