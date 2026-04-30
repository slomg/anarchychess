using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Errors;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Entities;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.QuestTests;

public class QuestServiceTests : BaseIntegrationTest
{
    private readonly IQuestService _questService;

    public QuestServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _questService = Scope.ServiceProvider.GetRequiredService<IQuestService>();
    }

    [Fact]
    public async Task GetPaginatedMonthlyLeaderboardAsync_applies_pagination()
    {
        List<UserQuestPoints> questPoints =
        [
            new UserQuestPointsFaker().RuleFor(x => x.MonthlyPoints, 4).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.MonthlyPoints, 3).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.MonthlyPoints, 2).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.MonthlyPoints, 1).Generate(),
        ];
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var result = await _questService.GetPaginatedMonthlyLeaderboardAsync(pagination, CT);

        var expected = questPoints[..3]
            .Select(x => new QuestPointsDto(
                new MinimalProfile(x.User),
                MonthlyQuestPoints: x.MonthlyPoints,
                TotalQuestPoints: x.TotalPoints
            ))
            .ToList();
        result.Items.Should().BeEquivalentTo(expected);
        result.TotalCount.Should().Be(questPoints.Count);
        result.Page.Should().Be(pagination.Page);
        result.PageSize.Should().Be(pagination.PageSize);
    }

    [Fact]
    public async Task GetPaginatedTotalLeaderboardAsync_applies_pagination()
    {
        List<UserQuestPoints> questPoints =
        [
            new UserQuestPointsFaker().RuleFor(x => x.TotalPoints, 4).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.TotalPoints, 3).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.TotalPoints, 2).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.TotalPoints, 1).Generate(),
        ];
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var result = await _questService.GetPaginatedTotalLeaderboardAsync(pagination, CT);

        var expected = questPoints[..3]
            .Select(x => new QuestPointsDto(
                new MinimalProfile(x.User),
                MonthlyQuestPoints: x.MonthlyPoints,
                TotalQuestPoints: x.TotalPoints
            ))
            .ToList();
        result.Items.Should().BeEquivalentTo(expected);
        result.TotalCount.Should().Be(questPoints.Count);
        result.Page.Should().Be(pagination.Page);
        result.PageSize.Should().Be(pagination.PageSize);
    }

    [Fact]
    public async Task GetMyRankingAsync_returns_correct_monthly_and_total_ranking()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = questPoints[2];
        var result = await _questService.GetMyRankingAsync(testPoints.UserId, CT);

        result.MonthlyQuestPoints.Should().Be(testPoints.MonthlyPoints);
        result.TotalQuestPoints.Should().Be(testPoints.TotalPoints);
        result
            .MonthlyRank.Should()
            .Be(questPoints.Count(u => u.MonthlyPoints > testPoints.MonthlyPoints) + 1);
        result
            .TotalRank.Should()
            .Be(questPoints.Count(u => u.TotalPoints > testPoints.TotalPoints) + 1);
    }

    [Fact]
    public async Task GetMyRankingAsync_returns_zero_points_and_last_place_when_no_points()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _questService.GetMyRankingAsync(user.Id, CT);

        result.MonthlyQuestPoints.Should().Be(0);
        result.TotalQuestPoints.Should().Be(0);
        result.MonthlyRank.Should().Be(questPoints.Count(u => u.MonthlyPoints > 0) + 1);
        result.TotalRank.Should().Be(questPoints.Count + 1);
    }

    [Fact]
    public async Task GetQuestPointsAsync_returns_points_when_found()
    {
        var existing = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _questService.GetTotalQuestPointsAsync(existing.UserId, CT);

        result.Should().Be(existing.TotalPoints);
    }

    [Fact]
    public async Task GetQuestPointsAsync_returns_zero_when_no_points()
    {
        var existing = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _questService.GetTotalQuestPointsAsync(existing.UserId, CT);

        result.Should().Be(0);
    }

    [Fact]
    public async Task IncrementQuestPointsAsync_returns_error_when_user_is_guest()
    {
        UserId guestId = UserId.Guest();

        var result = await _questService.IncrementQuestPointsAsync(guestId, 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ProfileErrors.NotFound);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should().BeEmpty();
    }

    [Fact]
    public async Task IncrementQuestPointsAsync_adds_when_not_found()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);
        int points = 123;

        var result = await _questService.IncrementQuestPointsAsync(user.Id, points, CT);

        result.IsError.Should().BeFalse();

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new UserQuestPoints
                {
                    UserId = user.Id,
                    User = user,
                    MonthlyPoints = points,
                    TotalPoints = points,
                }
            );
    }

    [Fact]
    public async Task IncrementQuestPointsAsync_updates_when_found()
    {
        var existing = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        int incrementBy = 100;
        int exectedMonthly = existing.MonthlyPoints + incrementBy;
        int expectedTotal = existing.TotalPoints + incrementBy;

        var result = await _questService.IncrementQuestPointsAsync(
            existing.UserId,
            incrementBy,
            CT
        );

        result.IsError.Should().BeFalse();
        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new UserQuestPoints
                {
                    UserId = existing.UserId,
                    User = existing.User,
                    MonthlyPoints = exectedMonthly,
                    TotalPoints = expectedTotal,
                }
            );
    }

    [Fact]
    public async Task ResetMonthlyPointsAsync_zeroes_monthly_points_and_preserves_total()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        await _questService.ResetMonthlyPointsAsync(CT);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should().HaveCount(questPoints.Count);
        inDb.Should().AllSatisfy(x => x.MonthlyPoints.Should().Be(0));
        inDb.Should()
            .BeEquivalentTo(questPoints, options => options.Excluding(x => x.MonthlyPoints));
    }
}
