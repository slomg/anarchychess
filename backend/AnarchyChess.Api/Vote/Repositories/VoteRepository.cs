using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Vote.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Vote.Repositories;

public interface IVoteRepository
{
    void AddUserVote(UserVote vote);
    Task<VoteOptionPair?> GetNextPairAsync(
        UserId userId,
        string? ip,
        CancellationToken token = default
    );
    Task<PendingUserVote?> GetUserPendingVoteAsync(
        UserId userId,
        CancellationToken token = default
    );
    void RemovePendingUserVote(PendingUserVote pendingVote);
    void AddPendingUserVote(PendingUserVote pendingVote);
    Task BulkAddVoteOptionPairsIfNotExistAsync(
        IEnumerable<VoteOptionPair> pairs,
        CancellationToken token = default
    );
    Task BulkAddVoteOptionsIfNotExistAsync(
        IEnumerable<VoteOption> options,
        CancellationToken token = default
    );
}

public class VoteRepository(ApplicationDbContext dbContext) : IVoteRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<PendingUserVote?> GetUserPendingVoteAsync(
        UserId userId,
        CancellationToken token = default
    ) => _dbContext.PendingUserVotes.FirstOrDefaultAsync(x => x.UserId == userId, token);

    public void AddPendingUserVote(PendingUserVote pendingVote) =>
        _dbContext.PendingUserVotes.Add(pendingVote);

    public void RemovePendingUserVote(PendingUserVote pendingVote) =>
        _dbContext.PendingUserVotes.Remove(pendingVote);

    public void AddUserVote(UserVote vote) => _dbContext.UserVotes.Add(vote);

    public Task BulkAddVoteOptionPairsIfNotExistAsync(
        IEnumerable<VoteOptionPair> pairs,
        CancellationToken token = default
    ) =>
        _dbContext.BulkInsertOrUpdateAsync(
            pairs,
            config =>
            {
                config.PropertiesToInclude = [];
                config.UpdateByProperties =
                [
                    nameof(VoteOptionPair.OptionAKey),
                    nameof(VoteOptionPair.OptionBKey),
                ];
            },
            cancellationToken: token
        );

    public Task BulkAddVoteOptionsIfNotExistAsync(
        IEnumerable<VoteOption> options,
        CancellationToken token = default
    ) =>
        _dbContext.BulkInsertOrUpdateAsync(
            options,
            config =>
            {
                config.PropertiesToInclude = [];
            },
            cancellationToken: token
        );

    public Task<VoteOptionPair?> GetNextPairAsync(
        UserId userId,
        string? ip,
        CancellationToken token = default
    ) =>
        _dbContext
            .VoteOptionPairs.Where(pair =>
                !_dbContext.UserVotes.Any(vote =>
                    vote.VotePair.Id == pair.Id
                    && (vote.UserId == userId || (ip != null && vote.IpAddress == ip))
                )
            )
            .OrderBy(_ => EF.Functions.Random())
            .FirstOrDefaultAsync(token);
}
