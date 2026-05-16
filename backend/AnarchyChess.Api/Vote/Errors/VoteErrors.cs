using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.Vote.Errors;

public static class VoteErrors
{
    public static readonly Error NoUnseenPairFound = Error.NotFound(
        ErrorCodes.VoteNoUnseenPairFound,
        "No unseen pair found"
    );

    public static readonly Error NoPendingVote = Error.Forbidden(
        ErrorCodes.VoteNoPendingVote,
        "No pending vote"
    );

    public static readonly Error InvalidVote = Error.Forbidden(
        ErrorCodes.VoteInvalid,
        "Vote is invalid"
    );
}
