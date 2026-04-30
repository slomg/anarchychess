using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Errors;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Entities;
using AnarchyChess.Api.Quests.Repositories;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace AnarchyChess.Api.Quests.Services;

public interface IQuestService
{
    Task<PagedResult<QuestPointsDto>> GetPaginatedMonthlyLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<PagedResult<QuestPointsDto>> GetPaginatedTotalLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<QuestRankingDto> GetRankingAsync(UserId userId, CancellationToken token = default);
    Task<ErrorOr<Updated>> IncrementQuestPointsAsync(
        UserId userId,
        int points,
        CancellationToken token = default
    );
    Task ResetMonthlyPointsAsync(CancellationToken token = default);
    Task<int> GetTotalQuestPointsAsync(UserId userId, CancellationToken token = default);
}

public class QuestService(
    IQuestRepository questRepository,
    UserManager<AuthedUser> userManager,
    IUnitOfWork unitOfWork
) : IQuestService
{
    private readonly IQuestRepository _questRepository = questRepository;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<QuestPointsDto>> GetPaginatedMonthlyLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var questPoints = await _questRepository.GetPaginatedMonthlyLeaderboardAsync(
            pagination,
            token
        );
        var totalCount = await _questRepository.GetMonthlyCountAsync(token);

        return new(
            Items: questPoints.Select(questPoint => new QuestPointsDto(
                new MinimalProfile(questPoint.User),
                MonthlyQuestPoints: questPoint.MonthlyPoints,
                TotalQuestPoints: questPoint.TotalPoints
            )),
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public async Task<QuestRankingDto> GetRankingAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        var questPoints = await _questRepository.GetUserPointsAsync(userId, token);
        int totalRank = await _questRepository.GetTotalRankingAsync(
            questPoints?.TotalPoints ?? 0,
            token
        );
        int monthlyRank = await _questRepository.GetMonthlyRankingAsync(
            questPoints?.MonthlyPoints ?? 0,
            token
        );

        return new(
            TotalQuestPoints: questPoints?.TotalPoints ?? 0,
            TotalRank: totalRank,
            MonthlyQuestPoints: questPoints?.MonthlyPoints ?? 0,
            MonthlyRank: monthlyRank
        );
    }

    public async Task<int> GetTotalQuestPointsAsync(
        UserId userId,
        CancellationToken token = default
    )
    {
        var questPoints = await _questRepository.GetUserPointsAsync(userId, token);
        return questPoints?.TotalPoints ?? 0;
    }

    public async Task<PagedResult<QuestPointsDto>> GetPaginatedTotalLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var questPoints = await _questRepository.GetPaginatedTotalLeaderboardAsync(
            pagination,
            token
        );
        var totalCount = await _questRepository.GetTotalCountAsync(token);

        return new(
            Items: questPoints.Select(questPoint => new QuestPointsDto(
                new MinimalProfile(questPoint.User),
                MonthlyQuestPoints: questPoint.MonthlyPoints,
                TotalQuestPoints: questPoint.TotalPoints
            )),
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    public async Task<ErrorOr<Updated>> IncrementQuestPointsAsync(
        UserId userId,
        int incrementBy,
        CancellationToken token = default
    )
    {
        if (userId.IsGuest)
        {
            return ProfileErrors.NotFound;
        }

        var userQuestPoints = await _questRepository.GetUserPointsAsync(userId, token);
        if (userQuestPoints is not null)
        {
            userQuestPoints.MonthlyPoints += incrementBy;
            userQuestPoints.TotalPoints += incrementBy;
            await _unitOfWork.CompleteAsync(token);
            return Result.Updated;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ProfileErrors.NotFound;
        }

        await _questRepository.AddQuestPointsAsync(
            new UserQuestPoints()
            {
                UserId = user.Id,
                User = user,
                MonthlyPoints = incrementBy,
                TotalPoints = incrementBy,
            },
            token
        );
        await _unitOfWork.CompleteAsync(token);
        return Result.Updated;
    }

    public async Task ResetMonthlyPointsAsync(CancellationToken token = default)
    {
        await _questRepository.ResetMonthlyAsync(token);
        await _unitOfWork.CompleteAsync(token);
    }
}
