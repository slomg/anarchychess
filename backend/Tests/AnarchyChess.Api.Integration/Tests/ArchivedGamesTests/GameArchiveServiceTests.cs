using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.ArchivedGames.Models;
using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.ArchivedGamesTests;

public class GameArchiveServiceTests : BaseIntegrationTest
{
    private readonly IGameArchiveService _gameArchiveService;

    private readonly GameToken _gameToken = "test game token";

    public GameArchiveServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameArchiveService = Scope.ServiceProvider.GetRequiredService<IGameArchiveService>();
    }

    [Fact]
    public async Task CreateHumanArchiveAsync_creates_and_saves_the_game_archive_correctly()
    {
        var pool = new PoolKeyFaker().Generate();
        var whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
        var blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();
        GameEndStatus endStatus = new(GameResult.WhiteWin, "White Won by Resignation");

        var result = await _gameArchiveService.CreateHumanArchiveAsync(
            _gameToken,
            pool,
            whitePlayer,
            blackPlayer,
            endStatus,
            CT
        );

        await DbContext.SaveChangesAsync(CT);

        var savedArchive = await GetSavedArchiveAsync(_gameToken);

        savedArchive.Should().BeEquivalentTo(result);
        GameArchive expectedArchive = new()
        {
            GameToken = _gameToken,
            Result = endStatus.Result,
            ResultDescription = endStatus.ResultDescription,

            IsBotGame = false,
            PoolType = pool.PoolType,
            BaseSeconds = pool.TimeControl.BaseSeconds,
            IncrementSeconds = pool.TimeControl.IncrementSeconds,

            WhitePlayer = CreateExpectedPlayerArchive(player: whitePlayer),
            BlackPlayer = CreateExpectedPlayerArchive(player: blackPlayer),
        };

        savedArchive
            .Should()
            .BeEquivalentTo(
                expectedArchive,
                options =>
                    options
                        .Excluding(x => x.CreatedAt)
                        .Excluding(x => x.WhitePlayerId)
                        .Excluding(x => x.WhitePlayer.Id)
                        .Excluding(x => x.BlackPlayerId)
                        .Excluding(x => x.BlackPlayer.Id)
            );
    }

    [Fact]
    public async Task CreateBotArchiveAsync_sets_IsBotGame_to_true()
    {
        var result = await _gameArchiveService.CreateBotArchiveAsync(
            _gameToken,
            new PoolKeyFaker().Generate(),
            new GamePlayerFaker(GameColor.White).Generate(),
            new GamePlayerFaker(GameColor.Black).Generate(),
            new(GameResult.WhiteWin, "test"),
            CT
        );

        result.IsBotGame.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_returns_expected_metadata_and_items()
    {
        var userId = "user123";
        var archives = new GameArchiveFaker(whiteUserId: userId)
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate(4);
        await DbContext.GameArchives.AddRangeAsync(archives, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 2);

        var result = await _gameArchiveService.GetPaginatedResultsAsync(userId, pagination, CT);

        result.Page.Should().Be(0);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(4);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(dto => archives.Any(a => a.GameToken == dto.GameToken));
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_returns_empty_when_user_has_no_archives()
    {
        PaginationQuery pagination = new(Page: 0, PageSize: 5);

        var result = await _gameArchiveService.GetPaginatedResultsAsync("no one", pagination, CT);

        result.Page.Should().Be(0);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_maps_all_properties_correctly()
    {
        var archive = new GameArchiveFaker()
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate();
        await DbContext.AddAsync(archive, CT);
        await DbContext.SaveChangesAsync(CT);

        var pagination = new PaginationQuery(Page: 0, PageSize: 1);
        var result = await _gameArchiveService.GetPaginatedResultsAsync(
            archive.BlackPlayer.UserId,
            pagination,
            CT
        );

        result.Items.Should().ContainSingle();
        var summary = result.Items.Single();

        GameSummaryDto expectedSummary = new(
            GameToken: archive.GameToken,
            WhitePlayer: new PlayerSummaryDto(
                UserId: archive.WhitePlayer.UserId,
                UserName: archive.WhitePlayer.UserName
            ),
            BlackPlayer: new PlayerSummaryDto(
                UserId: archive.BlackPlayer.UserId,
                UserName: archive.BlackPlayer.UserName
            ),
            PoolType: archive.PoolType,
            BaseSeconds: archive.BaseSeconds,
            IncrementSeconds: archive.IncrementSeconds,
            IsBotGame: archive.IsBotGame,
            Result: archive.Result,
            CreatedAt: archive.CreatedAt
        );
        summary.Should().BeEquivalentTo(expectedSummary);
    }

    private async Task<GameArchive> GetSavedArchiveAsync(GameToken gameToken)
    {
        var archive = await DbContext
            .GameArchives.Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .FirstOrDefaultAsync(g => g.GameToken == gameToken, CT);

        archive.Should().NotBeNull();
        return archive;
    }

    private static PlayerArchive CreateExpectedPlayerArchive(GamePlayer player) =>
        new()
        {
            UserId = player.UserId,
            UserName = player.UserName,
            Color = player.Color,
        };
}
