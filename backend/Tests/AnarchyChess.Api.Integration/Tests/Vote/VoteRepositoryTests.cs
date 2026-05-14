using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.Vote.Repositories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.Vote;

public class VoteRepositoryTests : BaseIntegrationTest
{
    private readonly IVoteRepository _repository;

    public VoteRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IVoteRepository>();
    }

    [Fact]
    public async Task GetUserPendingVoteAsync_returns_vote_for_user()
    {
        var user = new AuthedUserFaker().Generate();
        var pending = new PendingUserVoteFaker(user.Id).Generate();
        var other = new PendingUserVoteFaker().Generate();
        await DbContext.AddRangeAsync(pending, other);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserPendingVoteAsync(user.Id, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(pending);
    }

    [Fact]
    public async Task GetUserPendingVoteAsync_returns_null_when_missing()
    {
        var user = new AuthedUserFaker().Generate();
        var other = new PendingUserVoteFaker().Generate();

        await DbContext.AddAsync(other, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserPendingVoteAsync(user.Id, CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddPendingUserVote_adds_pending_vote()
    {
        var pending = new PendingUserVoteFaker().Generate();

        _repository.AddPendingUserVote(pending);
        await DbContext.SaveChangesAsync(CT);

        var db = await DbContext.PendingUserVotes.AsNoTracking().SingleAsync(CT);

        db.Should().BeEquivalentTo(pending);
    }

    [Fact]
    public async Task RemovePendingUserVote_removes_pending_vote()
    {
        var toRemove = new PendingUserVoteFaker().Generate();
        var other = new PendingUserVoteFaker().Generate();
        await DbContext.AddRangeAsync(toRemove, other);
        await DbContext.SaveChangesAsync(CT);

        _repository.RemovePendingUserVote(toRemove);
        await DbContext.SaveChangesAsync(CT);

        var remaining = await DbContext.PendingUserVotes.AsNoTracking().ToListAsync(CT);

        remaining.Should().ContainSingle().Which.Should().BeEquivalentTo(other);
    }

    [Fact]
    public async Task AddUserVote_adds_vote()
    {
        var vote = new UserVoteFaker().Generate();

        _repository.AddUserVote(vote);
        await DbContext.SaveChangesAsync(CT);

        var db = await DbContext.UserVotes.AsNoTracking().SingleAsync(CT);

        db.Should().BeEquivalentTo(vote);
    }

    [Fact]
    public async Task GetNextPairAsync_excludes_pairs_already_voted_by_user()
    {
        var user = new AuthedUserFaker().Generate();

        var pair1 = new VoteOptionPairFaker().Generate();
        var pair2 = new VoteOptionPairFaker().Generate();

        var vote = new UserVoteFaker(userId: user.Id, pair: pair1).Generate();

        await DbContext.AddRangeAsync(pair1, pair2, vote);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetNextPairAsync(user.Id, ip: null, CT);

        result.Should().NotBeNull();
        result.Id.Should().Be(pair2.Id);
    }

    [Fact]
    public async Task GetNextPairAsync_excludes_pairs_voted_by_same_ip()
    {
        string ip = "1.1.1.1";

        var pair1 = new VoteOptionPairFaker().Generate();
        var pair2 = new VoteOptionPairFaker().Generate();

        var vote = new UserVoteFaker(pair: pair1).RuleFor(x => x.IpAddress, ip).Generate();

        await DbContext.AddRangeAsync(pair1, pair2, vote);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetNextPairAsync("different user", ip, CT);

        result.Should().NotBeNull();
        result.Id.Should().Be(pair2.Id);
    }

    [Fact]
    public async Task GetNextPairAsync_returns_null_when_there_are_no_more_pairs_left()
    {
        var pair1 = new VoteOptionPairFaker().Generate();
        var pair2 = new VoteOptionPairFaker().Generate();
        var vote1 = new UserVoteFaker(pair: pair1).Generate();
        var vote2 = new UserVoteFaker(userId: vote1.UserId, pair: pair1).Generate();
        await DbContext.AddRangeAsync(pair1, pair2, vote1, vote2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetNextPairAsync(vote1.UserId, ip: null, CT);

        result.Should().BeNull();
    }
}
