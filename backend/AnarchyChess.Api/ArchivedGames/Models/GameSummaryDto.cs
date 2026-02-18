using System.ComponentModel;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.ArchivedGames.Models;

[DisplayName("GameSummary")]
public record GameSummaryDto(
    GameToken GameToken,
    PlayerSummaryDto WhitePlayer,
    PlayerSummaryDto BlackPlayer,
    PoolType PoolType,
    int BaseSeconds,
    int IncrementSeconds,
    GameResult Result,
    DateTime CreatedAt
);
