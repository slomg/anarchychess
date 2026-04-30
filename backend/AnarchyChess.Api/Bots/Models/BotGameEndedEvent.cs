using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Bots.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Bots.Models.BotGameEndedEvent")]
public record BotGameEndedEvent(GameToken GameToken, GameResultData EndStatus);
