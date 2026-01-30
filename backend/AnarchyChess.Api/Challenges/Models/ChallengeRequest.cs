using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Challenges.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Challenges.Models.IncomingChallenge")]
public record ChallengeRequest(
    [property: Id(0)] ChallengeToken ChallengeToken,
    [property: Id(1)] MinimalProfile Requester,
    [property: Id(2)] MinimalProfile? Recipient,
    [property: Id(3)] PoolKey Pool,
    [property: Id(4)] DateTime ExpiresAt,
    [property: Id(5)] UserId? CancelledBy = null,
    [property: Id(6)] GameToken? ResolvedGame = null
);
