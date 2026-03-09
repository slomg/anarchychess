using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Profile.Services;

public interface IBotProfilePictureProvider
{
    byte[]? GetBotProfilePictureBytes(UserId userId);
    DateTimeOffset GetBotProfilePictureLastModified(UserId userId);
}

public record BotProfilePicture(byte[] ImageBytes, DateTimeOffset LastModified);

public class BotProfilePictureProvider : IBotProfilePictureProvider
{
    private static readonly string _baseDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Bots"
    );

    private readonly Dictionary<UserId, BotProfilePicture> _botIdToPicture = new()
    {
        [UserId.AnarchyBot()] = LoadProfilePicture("anarchybot.webp"),
        [UserId.LobotomizedAnarchyBot()] = LoadProfilePicture("lobotomized-anarchybot.webp"),
    };

    private static BotProfilePicture LoadProfilePicture(string name)
    {
        string path = Path.Combine(_baseDirectory, name);
        byte[] bytes = File.ReadAllBytes(path);
        DateTimeOffset lastModified = File.GetLastWriteTimeUtc(path);

        return new(ImageBytes: bytes, LastModified: lastModified);
    }

    public byte[]? GetBotProfilePictureBytes(UserId userId) =>
        _botIdToPicture.GetValueOrDefault(userId)?.ImageBytes;

    public DateTimeOffset GetBotProfilePictureLastModified(UserId userId) =>
        _botIdToPicture.GetValueOrDefault(userId)?.LastModified ?? DateTimeOffset.MinValue;
}
