using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.AnarchyBot.Errors;

public static class AnarchyBotErrors
{
    public static Error BotOffline =>
        Error.Failure(ErrorCodes.AnarchyBotOffline, "Bot is currently offline");

    public static Error NoMoveFound =>
        Error.Failure(
            ErrorCodes.AnarchyBotNoMove,
            "Bot could not find a move for the given position"
        );

    public static Error BotFailure =>
        Error.Failure(ErrorCodes.AnarchyBotFailure, "Failed to find move");
}
