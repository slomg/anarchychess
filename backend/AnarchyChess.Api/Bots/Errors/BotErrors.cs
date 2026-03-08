using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.Bots.Errors;

public static class BotErrors
{
    public static Error BotOffline =>
        Error.Failure(ErrorCodes.BotOffline, "Bot is currently offline");

    public static Error NoMoveFound =>
        Error.Failure(ErrorCodes.BotNoMove, "Bot could not find a move for the given position");

    public static Error BotFailure => Error.Failure(ErrorCodes.BotFailure, "Failed to find move");
}
