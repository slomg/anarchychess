using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.AnarchyBot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.AnarchyBot.Models.AnarchyBotGameState")]
public record AnarchyBotGameState(
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    GameColor SideToMove,
    string InitialFen,
    IReadOnlyList<MoveSnapshot> MoveHistory,
    IReadOnlyCollection<MovePath> LegalMoves,
    GameResultData? ResultData
);
