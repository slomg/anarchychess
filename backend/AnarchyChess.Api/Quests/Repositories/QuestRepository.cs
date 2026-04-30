using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Pagination.Extensions;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Quests.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Quests.Repositories;

public interface IQuestRepository
{
    Task AddQuestPointsAsync(UserQuestPoints questPoints, CancellationToken token = default);
    Task ResetMonthlyAsync(CancellationToken token = default);
    Task<List<UserQuestPoints>> GetPaginatedMonthlyLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetMonthlyRankingAsync(int points, CancellationToken token = default);
    Task<int> GetMonthlyCountAsync(CancellationToken token = default);
    Task<UserQuestPoints?> GetUserPointsAsync(UserId userId, CancellationToken token = default);
    Task<List<UserQuestPoints>> GetPaginatedTotalLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> GetTotalCountAsync(CancellationToken token = default);
    Task<int> GetTotalRankingAsync(int points, CancellationToken token = default);
}

public class QuestRepository(ApplicationDbContext dbContext) : IQuestRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<UserQuestPoints>> GetPaginatedTotalLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .QuestPoints.OrderByDescending(x => x.TotalPoints)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetTotalCountAsync(CancellationToken token = default) =>
        _dbContext.QuestPoints.CountAsync(token);

    public async Task<int> GetTotalRankingAsync(int points, CancellationToken token = default) =>
        await _dbContext.QuestPoints.CountAsync(x => x.TotalPoints > points, token) + 1;

    public Task<List<UserQuestPoints>> GetPaginatedMonthlyLeaderboardAsync(
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .QuestPoints.Where(x => x.MonthlyPoints > 0)
            .OrderByDescending(x => x.MonthlyPoints)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> GetMonthlyCountAsync(CancellationToken token = default) =>
        _dbContext.QuestPoints.Where(x => x.MonthlyPoints > 0).CountAsync(token);

    public async Task<int> GetMonthlyRankingAsync(int points, CancellationToken token = default) =>
        await _dbContext
            .QuestPoints.Where(x => x.MonthlyPoints > 0)
            .CountAsync(x => x.MonthlyPoints > points, token) + 1;

    public Task<UserQuestPoints?> GetUserPointsAsync(
        UserId userId,
        CancellationToken token = default
    ) => _dbContext.QuestPoints.FirstOrDefaultAsync(x => x.UserId == userId, token);

    public async Task AddQuestPointsAsync(
        UserQuestPoints questPoints,
        CancellationToken token = default
    ) => await _dbContext.AddAsync(questPoints, token);

    public Task ResetMonthlyAsync(CancellationToken token = default) =>
        _dbContext.QuestPoints.ExecuteUpdateAsync(
            x => x.SetProperty(p => p.MonthlyPoints, 0),
            token
        );
}
