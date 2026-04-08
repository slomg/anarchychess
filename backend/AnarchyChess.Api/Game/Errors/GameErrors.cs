using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.Game.Errors;

public static class GameErrors
{
    public static Error GameNotFound =>
        Error.NotFound(ErrorCodes.GameNotFound, "Game with that token doesn't exist");

    public static Error PlayerInvalid =>
        Error.Forbidden(
            ErrorCodes.GamePlayerInvalid,
            "The provided player is unable to perform the requested action"
        );

    public static Error MoveInvalid =>
        Error.Forbidden(ErrorCodes.GameMoveInvalid, "The provided move is illegal");

    public static Error DrawAlreadyRequested =>
        Error.Forbidden(
            ErrorCodes.GameDrawAlreadyRequested,
            "You already have a pending draw request"
        );

    public static Error DrawOnCooldown =>
        Error.Forbidden(
            ErrorCodes.GameDrawOnCooldown,
            "You cannot a draw request as you are on cooldown"
        );

    public static Error DrawNotRequested =>
        Error.Forbidden(
            ErrorCodes.GameDrawNotRequested,
            "You cannot decline a draw that doesn't exist"
        );

    public static Error GameNotOver =>
        Error.Conflict(ErrorCodes.GameNotOver, "The game is still active");

    public static Error InvalidPieceLetter =>
        Error.Validation(ErrorCodes.GameInvalidPieceLetter, "A provided piece letter is invalid");

    public static Error MalformedFenParts =>
        Error.Validation(
            ErrorCodes.GameMalformedFenParts,
            "The provided fen is malformed, must have the correct number of parts separated by a space"
        );

    public static Error MalformedFenPieces =>
        Error.Validation(ErrorCodes.GameMalformedFenPieces, "The provided fen pieces is malformed");

    public static Error MalformedFenMovedPieces =>
        Error.Validation(
            ErrorCodes.GameMalformedFenMovedPieces,
            "The provided fen moved pieces is malformed"
        );

    public static Error MalformedFenStunnedPieces =>
        Error.Validation(
            ErrorCodes.GameMalformedFenStunnedPieces,
            "The provided fen moved pieces is malformed"
        );

    public static Error MalformedFenLastMove =>
        Error.Validation(
            ErrorCodes.GameMalformedFenLastMove,
            "The provided fen last move is malformed"
        );
}
