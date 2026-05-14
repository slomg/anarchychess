using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.Vote.DTOs;
using AnarchyChess.Api.Vote.Entities;
using AnarchyChess.Api.Vote.Errors;
using AnarchyChess.Api.Vote.Services;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.Vote;

public class VoteServiceTests : BaseIntegrationTest
{
    private readonly IVoteService _voteService;
    private const string IP = "1.1.1.1";

    public VoteServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _voteService = Scope.ServiceProvider.GetRequiredService<IVoteService>();
    }

    [Fact]
    public async Task SelectNextPairAsync_returns_existing_pending_vote_as_dto()
    {
        var pending = new PendingUserVoteFaker().Generate();
        await DbContext.AddAsync(pending, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.SelectNextPairAsync(pending.UserId, IP, CT);

        result.IsError.Should().BeFalse();
        PendingUserVoteDto expected = new(
            new VoteOptionDto(
                pending.VotePair.OptionA.Id,
                pending.VotePair.OptionA.Name,
                pending.VotePair.OptionA.Description
            ),
            new VoteOptionDto(
                pending.VotePair.OptionB.Id,
                pending.VotePair.OptionB.Name,
                pending.VotePair.OptionB.Description
            )
        );
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task SelectNextPairAsync_returns_error_when_no_pairs_exist()
    {
        var user = new AuthedUserFaker().Generate();

        var result = await _voteService.SelectNextPairAsync(user.Id, IP, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(VoteErrors.NoUnseenPairFound);
    }

    [Fact]
    public async Task SelectNextPairAsync_creates_pending_vote_and_returns_dto()
    {
        var user = new AuthedUserFaker().Generate();
        var pair = new VoteOptionPairFaker().Generate();
        await DbContext.AddAsync(pair, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.SelectNextPairAsync(user.Id, IP, CT);

        result.IsError.Should().BeFalse();

        var inDb = await DbContext.PendingUserVotes.AsNoTracking().SingleAsync(CT);
        inDb.UserId.Should().Be(user.Id);
        inDb.VotePair.Id.Should().Be(pair.Id);

        var expected = new PendingUserVoteDto(
            new VoteOptionDto(pair.OptionA.Id, pair.OptionA.Name, pair.OptionA.Description),
            new VoteOptionDto(pair.OptionB.Id, pair.OptionB.Name, pair.OptionB.Description)
        );

        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task SelectNextPairAsync_guest_user_returns_error_when_ip_and_user_already_block_all_pairs()
    {
        UserId guestId = UserId.Guest();

        var pair1 = new VoteOptionPairFaker().Generate();
        var pair2 = new VoteOptionPairFaker().Generate();

        var guestVote = new UserVoteFaker(guestId, pair: pair1).Generate();
        var ipVote = new UserVoteFaker(pair: pair2).RuleFor(x => x.IpAddress, IP).Generate();
        await DbContext.AddRangeAsync(pair1, pair2, guestVote, ipVote);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.SelectNextPairAsync(guestId, IP, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(VoteErrors.NoUnseenPairFound);
    }

    [Fact]
    public async Task SelectNextPairAsync_authed_user_ignores_ip_and_returns_remaining_pair()
    {
        var user = new AuthedUserFaker().Generate();

        var pair1 = new VoteOptionPairFaker().Generate();
        var pair2 = new VoteOptionPairFaker().Generate();

        var userVote = new UserVoteFaker(user.Id, pair1).Generate();
        var ipVote = new UserVoteFaker(pair: pair2).RuleFor(x => x.IpAddress, IP).Generate();
        await DbContext.AddRangeAsync(pair1, pair2, userVote, ipVote);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.SelectNextPairAsync(user.Id, IP, CT);

        result.IsError.Should().BeFalse();

        result.Value.OptionA.OptionId.Should().Be(pair2.OptionA.Id);
        result.Value.OptionB.OptionId.Should().Be(pair2.OptionB.Id);
    }

    [Fact]
    public async Task CompleteVoteAsync_returns_error_when_no_pending_vote()
    {
        var user = new AuthedUserFaker().Generate();

        var result = await _voteService.CompleteVoteAsync(user.Id, IP, 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(VoteErrors.NoPendingVote);
    }

    [Fact]
    public async Task CompleteVoteAsync_returns_error_when_invalid_option()
    {
        var pending = new PendingUserVoteFaker().Generate();
        await DbContext.AddAsync(pending, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.CompleteVoteAsync(pending.UserId, IP, 999999, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(VoteErrors.InvalidVote);
    }

    [Fact]
    public async Task CompleteVoteAsync_creates_vote_removes_pending_and_sets_weight()
    {
        var pending = new PendingUserVoteFaker().Generate();
        await DbContext.AddAsync(pending, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.CompleteVoteAsync(
            pending.UserId,
            IP,
            pending.VotePair.OptionA.Id,
            CT
        );

        result.IsError.Should().BeFalse();

        var pendingDb = await DbContext.PendingUserVotes.AsNoTracking().ToListAsync(CT);
        pendingDb.Should().BeEmpty();

        var vote = await DbContext.UserVotes.AsNoTracking().SingleAsync(CT);

        vote.Should()
            .BeEquivalentTo(
                new UserVote()
                {
                    UserId = pending.UserId,
                    VotePairId = pending.VotePair.Id,
                    VotePair = pending.VotePair,
                    IpAddress = IP,
                    PickedOptionA = true,
                    VoteWeight = VoteService.AuthedWeight,
                },
                options => options.Excluding(x => x.CreatedAt)
            );
    }

    [Fact]
    public async Task CompleteVoteAsync_uses_guest_weight_for_guest_user()
    {
        UserId guestId = UserId.Guest();
        var pending = new PendingUserVoteFaker(guestId).Generate();
        await DbContext.AddAsync(pending, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.CompleteVoteAsync(
            guestId,
            IP,
            pending.VotePair.OptionA.Id,
            CT
        );

        result.IsError.Should().BeFalse();

        var vote = await DbContext.UserVotes.AsNoTracking().SingleAsync(CT);
        vote.VoteWeight.Should().Be(VoteService.GuestWeight);
    }

    [Fact]
    public async Task CompleteVoteAsync_creates_vote_with_optionB_when_optionA_not_selected()
    {
        var pending = new PendingUserVoteFaker().Generate();

        await DbContext.AddAsync(pending, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _voteService.CompleteVoteAsync(
            pending.UserId,
            IP,
            pending.VotePair.OptionB.Id,
            CT
        );

        result.IsError.Should().BeFalse();

        var vote = await DbContext.UserVotes.AsNoTracking().SingleAsync(CT);

        vote.UserId.Should().Be(pending.UserId);
        vote.VotePairId.Should().Be(pending.VotePair.Id);
        vote.PickedOptionA.Should().BeFalse();
    }
}
