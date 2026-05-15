using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Vote.DTOs;
using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Errors;
using AnarchyChess.Api.Vote.Models;
using AnarchyChess.Api.Vote.Repositories;
using ErrorOr;

namespace AnarchyChess.Api.Vote.Services;

public interface IVoteService
{
    Task<ErrorOr<Success>> CompleteVoteAsync(
        UserId userId,
        string ip,
        VoteOptionKey voteOptionKey,
        CancellationToken token = default
    );
    Task<ErrorOr<PendingUserVoteDto>> SelectNextPairAsync(
        UserId userId,
        string ip,
        CancellationToken token = default
    );
}

public class VoteService(IVoteRepository voteRepository, IUnitOfWork unitOfWork) : IVoteService
{
    public const float AuthedWeight = 1;
    public const float GuestWeight = 0.5f;

    private readonly IVoteRepository _voteRepository = voteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ErrorOr<PendingUserVoteDto>> SelectNextPairAsync(
        UserId userId,
        string ip,
        CancellationToken token = default
    )
    {
        var existingPendingVote = await _voteRepository.GetUserPendingVoteAsync(userId, token);
        if (existingPendingVote is not null)
        {
            return new PendingUserVoteDto(existingPendingVote);
        }

        var nextPair = await _voteRepository.GetNextPairAsync(
            userId,
            ip: userId.IsGuest ? ip : null,
            token
        );
        if (nextPair is null)
        {
            return VoteErrors.NoUnseenPairFound;
        }

        PendingUserVote pendingVote = new() { UserId = userId, VotePair = nextPair };
        _voteRepository.AddPendingUserVote(pendingVote);
        await _unitOfWork.CompleteAsync(token);

        return new PendingUserVoteDto(pendingVote);
    }

    public async Task<ErrorOr<Success>> CompleteVoteAsync(
        UserId userId,
        string ip,
        VoteOptionKey voteOptionKey,
        CancellationToken token = default
    )
    {
        var pendingVote = await _voteRepository.GetUserPendingVoteAsync(userId, token);
        if (pendingVote is null)
        {
            return VoteErrors.NoPendingVote;
        }

        if (
            voteOptionKey != pendingVote.VotePair.OptionAKey
            && voteOptionKey != pendingVote.VotePair.OptionBKey
        )
        {
            return VoteErrors.InvalidVote;
        }

        _voteRepository.RemovePendingUserVote(pendingVote);
        _voteRepository.AddUserVote(
            new()
            {
                UserId = userId,
                IpAddress = ip,
                VotePairId = pendingVote.VotePair.Id,
                VotePair = pendingVote.VotePair,
                PickedOptionA = voteOptionKey == pendingVote.VotePair.OptionAKey,
                VoteWeight = userId.IsAuthed ? AuthedWeight : GuestWeight,
            }
        );

        await _unitOfWork.CompleteAsync(token);

        return Result.Success;
    }
}
