using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.GamePlayer")]
public record GamePlayer(
    UserId UserId,
    GameColor Color,
    string UserName,
    string CountryCode,
    int? Rating
);
