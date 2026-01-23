using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.GameData")]
public class GameData
{
    [Id(1)]
    public required PlayerRoster Players { get; init; }

    [Id(2)]
    public required GameSource GameSource { get; init; }

    [Id(3)]
    public required PoolKey Pool { get; init; }

    [Id(4)]
    public required string InitialFen { get; init; }

    [Id(5)]
    public List<MoveSnapshot> MoveSnapshots { get; init; } = [];

    [Id(6)]
    public required GameCoreState Core { get; init; }

    [Id(7)]
    public DrawRequestState DrawRequest { get; init; } = new();

    [Id(8)]
    public required GameClockState ClockState { get; init; }

    [Id(9)]
    public GameNotifierState NotifierState { get; init; } = new();

    [Id(11)]
    public OvertimeState OvertimeState { get; init; } = new();

    [Id(10)]
    public GameResultData? Result { get; set; }
}

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.GameGrainState")]
public class GameGrainState
{
    [Id(0)]
    public GameData? CurrentGame { get; set; }
}
