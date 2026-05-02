using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.QuestLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.Models.GameQuestSnapshot")]
public record GameQuestSnapshot(
    GameColor PlayerColor,
    IReadOnlyChessBoard Board,
    GameResultData ResultData,
    PoolKey? Pool,
    ClockSnapshot? Clocks
);
