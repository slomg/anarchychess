using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Bots.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Bots.Models.BotGameState")]
public record BotGameState(
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    GameColor BotColor,
    GameColor SideToMove,
    string InitialFen,
    IReadOnlyList<MoveSnapshot> MoveHistory,
    IReadOnlyCollection<MovePath> LegalMoves,
    GameResultData? ResultData
);
