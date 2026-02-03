using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.PlayerRoster")]
public record PlayerRoster(GamePlayer WhitePlayer, GamePlayer BlackPlayer)
{
    public bool TryGetPlayerById(UserId? userId, [NotNullWhen(true)] out GamePlayer? player)
    {
        player = GetPlayerById(userId);
        return player is not null;
    }

    public GamePlayer? GetPlayerById(UserId? userId)
    {
        if (WhitePlayer.UserId == userId)
            return WhitePlayer;
        else if (BlackPlayer.UserId == userId)
            return BlackPlayer;
        return null;
    }

    public GamePlayer GetPlayerByColor(GameColor color) =>
        color.Match(whenWhite: WhitePlayer, whenBlack: BlackPlayer);
}
