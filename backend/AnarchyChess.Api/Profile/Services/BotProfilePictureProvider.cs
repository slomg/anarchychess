using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Profile.Services;

public interface IBotProfilePictureProvider
{
    byte[]? GetBotProfilePicture(UserId userId);
}

public class BotProfilePictureProvider : IBotProfilePictureProvider
{
    private static readonly string _baseDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "bot"
    );

    private readonly Dictionary<UserId, byte[]> _botIdToPicture = new()
    {
        [AnarchyBot.BotId] = LoadProfilePicture("anarchybot.webp"),
        [LobotomizedAnarchyBot.BotId] = LoadProfilePicture("lobotomized-anarchybot.webp"),
    };

    private static byte[] LoadProfilePicture(string name)
    {
        string path = Path.Combine(_baseDirectory, name);
        return File.ReadAllBytes(path);
    }

    public byte[]? GetBotProfilePicture(UserId userId)
    {
        if (!userId.IsBot)
        {
            return null;
        }

        return _botIdToPicture.GetValueOrDefault(userId);
    }
}
