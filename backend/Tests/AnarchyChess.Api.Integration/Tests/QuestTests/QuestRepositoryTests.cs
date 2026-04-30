using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Quests.Repositories;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.QuestTests;

public class QuestRepositoryTests : BaseIntegrationTest
{
    private readonly IQuestRepository _repository;

    public QuestRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IQuestRepository>();
    }

    [Fact]
    public async Task GetPaginatedMonthlyLeaderboardAsync_returns_correct_page_of_top_users()
    {
        int page = 1;
        int pageSize = 3;
        var questPoints = new UserQuestPointsFaker().RuleFor(x => x.TotalPoints, 0).Generate(10);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedMonthlyLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            CT
        );

        var expected = questPoints
            .OrderByDescending(x => x.MonthlyPoints)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetPaginatedTotalLeaderboardAsync_returns_correct_page_of_top_users()
    {
        int page = 1;
        int pageSize = 3;
        var questPoints = new UserQuestPointsFaker().RuleFor(x => x.MonthlyPoints, 0).Generate(10);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedTotalLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            CT
        );

        var expected = questPoints
            .OrderByDescending(x => x.TotalPoints)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMonthlyCountAsync_returns_the_number_of_users_with_monthly_points()
    {
        var activePoints = new UserQuestPointsFaker().Generate(3);
        var inactivePoints = new UserQuestPointsFaker()
            .RuleFor(x => x.MonthlyPoints, 0)
            .Generate(2);
        await DbContext.AddRangeAsync(activePoints, CT);
        await DbContext.AddRangeAsync(inactivePoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetMonthlyCountAsync(CT);
        result.Should().Be(activePoints.Count);
    }

    [Fact]
    public async Task GetTotalCountAsync_returns_the_number_of_users_with_quest_points()
    {
        var activePoints = new UserQuestPointsFaker().Generate(3);
        var inactivePoints = new UserQuestPointsFaker()
            .RuleFor(x => x.MonthlyPoints, 0)
            .Generate(2);
        await DbContext.AddRangeAsync(activePoints, CT);
        await DbContext.AddRangeAsync(inactivePoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTotalCountAsync(CT);
        result.Should().Be(activePoints.Count + inactivePoints.Count);
    }

    [Fact]
    public async Task GetMonthlyRankingAsync_finds_user_position()
    {
        var activePoints = new UserQuestPointsFaker().Generate(5);
        var inactivePoints = new UserQuestPointsFaker()
            .RuleFor(x => x.MonthlyPoints, 0)
            .Generate(3);
        await DbContext.AddRangeAsync([.. activePoints, .. inactivePoints], CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = activePoints[2];
        var result = await _repository.GetMonthlyRankingAsync(testPoints.MonthlyPoints, CT);

        result.Should().Be(activePoints.Count(u => u.MonthlyPoints > testPoints.MonthlyPoints) + 1);
    }

    [Fact]
    public async Task GetTotalRankingAsync_finds_user_position()
    {
        var activePoints = new UserQuestPointsFaker().Generate(5);
        var inactivePoints = new UserQuestPointsFaker()
            .RuleFor(x => x.MonthlyPoints, 0)
            .Generate(3);
        await DbContext.AddRangeAsync([.. activePoints, .. inactivePoints], CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = activePoints[2];
        var result = await _repository.GetTotalRankingAsync(testPoints.TotalPoints, CT);

        var allPoints = activePoints.Concat(inactivePoints).ToList();
        result.Should().Be(allPoints.Count(u => u.TotalPoints > testPoints.TotalPoints) + 1);
    }

    [Fact]
    public async Task GetUserPointsAsync_finds_user_points()
    {
        var points = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(points, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserPointsAsync(points.UserId, CT);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(points);
    }

    [Fact]
    public async Task AddQuestPointsAsync_adds_points()
    {
        var newPoints = new UserQuestPointsFaker().Generate();

        await _repository.AddQuestPointsAsync(newPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should().ContainSingle().Which.Should().BeEquivalentTo(newPoints);
    }

    [Fact]
    public async Task ResetMonthlyAsync_sets_all_monthly_points_to_zero()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        await _repository.ResetMonthlyAsync(CT);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should().HaveCount(questPoints.Count);
        inDb.Should().AllSatisfy(x => x.MonthlyPoints.Should().Be(0));
    }

    [Fact]
    public async Task ResetMonthlyAsync_preserves_total_points()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        await _repository.ResetMonthlyAsync(CT);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .BeEquivalentTo(questPoints, options => options.Excluding(x => x.MonthlyPoints));
    }
}
