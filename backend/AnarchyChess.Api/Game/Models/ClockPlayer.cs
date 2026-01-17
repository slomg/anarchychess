namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.ClockPlayer")]
public class ClockPlayer()
{
    [Id(0)]
    public double TimeLeftMs { get; set; }

    [Id(1)]
    public double? TimeUntilAbandonMs { get; set; }

    [Id(2)]
    public bool IsInGracePeriod { get; set; }
}
