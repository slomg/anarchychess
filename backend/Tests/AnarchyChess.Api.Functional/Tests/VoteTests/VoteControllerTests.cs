using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests.VoteTests;

public class VoteControllerTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetNextVotePair_returns_the_same_vote_for_the_same_user()
    {
        await DbContext.AddRangeAsync(new VoteOptionPairFaker().Generate(5), CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateAsync(ApiClient);

        var result1 = await ApiClient.Api.GetNextVotePairAsync();
        var result2 = await ApiClient.Api.GetNextVotePairAsync();

        result1.IsSuccessful.Should().BeTrue();
        result2.IsSuccessful.Should().BeTrue();

        result1.Content.Should().NotBeNull();
        result1.Content.Should().Be(result2.Content);
    }

    [Fact]
    public async Task GetNextVotePair_returns_the_same_vote_for_the_same_guest()
    {
        await DbContext.AddRangeAsync(new VoteOptionPairFaker().Generate(5), CT);
        await DbContext.SaveChangesAsync(CT);

        AuthUtils.AuthenticateGuest(ApiClient, UserId.Guest());

        var result1 = await ApiClient.Api.GetNextVotePairAsync();
        var result2 = await ApiClient.Api.GetNextVotePairAsync();

        result1.IsSuccessful.Should().BeTrue();
        result2.IsSuccessful.Should().BeTrue();

        result1.Content.Should().NotBeNull();
        result1.Content.Should().Be(result2.Content);
    }

    [Fact]
    public async Task CompleteVote_completes_vote_and_allows_picking_a_new_pair()
    {
        await DbContext.AddRangeAsync(new VoteOptionPairFaker().Generate(5), CT);
        await DbContext.SaveChangesAsync(CT);

        UserId guestId = UserId.Guest();
        AuthUtils.AuthenticateGuest(ApiClient, guestId);
        var guestNextResult1 = await ApiClient.Api.GetNextVotePairAsync();
        guestNextResult1.IsSuccessful.Should().BeTrue();

        await AuthUtils.AuthenticateAsync(ApiClient);
        var authNextResult1 = await ApiClient.Api.GetNextVotePairAsync();
        authNextResult1.IsSuccessful.Should().BeTrue();
        authNextResult1.Content.Should().NotBeNull();

        var completeResult = await ApiClient.Api.CompleteVoteAsync(
            authNextResult1.Content.OptionB.OptionKey
        );
        completeResult.IsSuccessful.Should().BeTrue();

        var authNextResult2 = await ApiClient.Api.GetNextVotePairAsync();
        authNextResult2.IsSuccessful.Should().BeTrue();
        authNextResult2.Content.Should().NotBe(authNextResult1.Content);

        AuthUtils.AuthenticateGuest(ApiClient, guestId);
        var guestNextResult2 = await ApiClient.Api.GetNextVotePairAsync();
        guestNextResult2.IsSuccessful.Should().BeTrue();
        guestNextResult2.Content.Should().Be(guestNextResult1.Content);
    }
}
