using System.Net;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Entities;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests;

public class QuestsControllerTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetDailyQuest_returns_a_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var quest1Response = await ApiClient.Api.GetDailyQuestAsync();
        quest1Response.IsSuccessful.Should().BeTrue();

        var quest2Response = await ApiClient.Api.GetDailyQuestAsync();
        quest2Response.IsSuccessful.Should().BeTrue();

        quest1Response.Content.Should().NotBeNull();
        quest1Response.Content.Should().BeEquivalentTo(quest2Response.Content);
    }

    [Fact]
    public async Task GetDailyQuest_returns_a_quest_for_guests()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var response = await ApiClient.Api.GetDailyQuestAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
    }

    [Fact]
    public async Task ReplaceDailyQuest_returns_a_new_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var quest1Response = await ApiClient.Api.GetDailyQuestAsync();
        quest1Response.IsSuccessful.Should().BeTrue();

        var replaceResponse = await ApiClient.Api.ReplaceDailyQuestAsync();
        replaceResponse.IsSuccessful.Should().BeTrue();

        replaceResponse.Content.Should().NotBeEquivalentTo(quest1Response.Content);

        var questAfterReplaceResponse = await ApiClient.Api.GetDailyQuestAsync();
        questAfterReplaceResponse.IsSuccessful.Should().BeTrue();

        questAfterReplaceResponse.Content.Should().BeEquivalentTo(replaceResponse.Content);
    }

    [Fact]
    public async Task ReplaceDailyQuest_returns_a_new_quest_for_guests()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var response = await ApiClient.Api.ReplaceDailyQuestAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
    }

    [Fact]
    public async Task CollectQuestReward_disallows_claiming_without_completing_a_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.CollectQuestRewardAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CollectQuestReward_allows_guests()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var response = await ApiClient.Api.CollectQuestRewardAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMonthlyQuestLeaderboard_returns_users_ordered_by_monthly_points()
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

        var response = await ApiClient.Api.GetMonthlyQuestLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: 3)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response.Content.TotalCount.Should().Be(questPoints.Count);
        response
            .Content.Items.Should()
            .BeEquivalentTo(
                questPoints[..3]
                    .Select(x => new QuestPointsDto(
                        new MinimalProfile(x.User),
                        MonthlyQuestPoints: x.MonthlyPoints,
                        TotalQuestPoints: x.TotalPoints
                    ))
            );
    }

    [Fact]
    public async Task GetMonthlyQuestLeaderboard_returns_bad_request_for_invalid_pagination()
    {
        var response = await ApiClient.Api.GetMonthlyQuestLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: -1)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTotalQuestLeaderboard_returns_users_ordered_by_total_points()
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

        var response = await ApiClient.Api.GetTotalQuestLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: 3)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response.Content.TotalCount.Should().Be(questPoints.Count);
        response
            .Content.Items.Should()
            .BeEquivalentTo(
                questPoints[..3]
                    .Select(x => new QuestPointsDto(
                        new MinimalProfile(x.User),
                        MonthlyQuestPoints: x.MonthlyPoints,
                        TotalQuestPoints: x.TotalPoints
                    ))
            );
    }

    [Fact]
    public async Task GetTotalQuestLeaderboard_returns_bad_request_for_invalid_pagination()
    {
        var response = await ApiClient.Api.GetTotalQuestLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: -1)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMyQuestRanking_returns_user_ranking()
    {
        var questPoints = new UserQuestPointsFaker()
            .RuleFor(x => x.MonthlyPoints, 10)
            .RuleFor(x => x.TotalPoints, 10)
            .Generate();
        var higherPoints = new UserQuestPointsFaker()
            .RuleFor(x => x.MonthlyPoints, 20)
            .RuleFor(x => x.TotalPoints, 20)
            .Generate(5);
        await DbContext.AddAsync(questPoints, CT);
        await DbContext.AddRangeAsync(higherPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, questPoints.User);

        var response = await ApiClient.Api.GetMyQuestRankingAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response.Content.MonthlyQuestPoints.Should().Be(questPoints.MonthlyPoints);
        response.Content.TotalQuestPoints.Should().Be(questPoints.TotalPoints);
        response.Content.MonthlyRank.Should().Be(higherPoints.Count + 1);
        response.Content.TotalRank.Should().Be(higherPoints.Count + 1);
    }

    [Fact]
    public async Task GetMyQuestRanking_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var response = await ApiClient.Api.GetMyQuestRankingAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
