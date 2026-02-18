using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.ArchivedGames.Models;
using AnarchyChess.Api.ArchivedGames.Repositories;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;

namespace AnarchyChess.Api.ArchivedGames.Services;

public interface IGameArchiveService
{
    Task<GameArchive> CreateArchiveAsync(
        GameToken gameToken,
        PoolKey pool,
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        GameEndStatus endStatus,
        CancellationToken token = default
    );
    Task<PagedResult<GameSummaryDto>> GetPaginatedResultsAsync(
        UserId userId,
        PaginationQuery pagination,
        CancellationToken token = default
    );
}

public class GameArchiveService(IGameArchiveRepository gameArchiveRepository) : IGameArchiveService
{
    private readonly IGameArchiveRepository _gameArchiveRepository = gameArchiveRepository;

    public async Task<GameArchive> CreateArchiveAsync(
        GameToken gameToken,
        PoolKey pool,
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        GameEndStatus endStatus,
        CancellationToken token = default
    )
    {
        var whiteArchive = CreatePlayerArchive(whitePlayer);
        var blackArchive = CreatePlayerArchive(blackPlayer);

        GameArchive gameArchive = new()
        {
            GameToken = gameToken,
            Result = endStatus.Result,
            ResultDescription = endStatus.ResultDescription,
            WhitePlayerId = whiteArchive.Id,
            WhitePlayer = whiteArchive,
            BlackPlayerId = blackArchive.Id,
            BlackPlayer = blackArchive,
            PoolType = pool.PoolType,
            BaseSeconds = pool.TimeControl.BaseSeconds,
            IncrementSeconds = pool.TimeControl.IncrementSeconds,
        };

        await _gameArchiveRepository.AddArchiveAsync(gameArchive, token);
        return gameArchive;
    }

    public async Task<PagedResult<GameSummaryDto>> GetPaginatedResultsAsync(
        UserId userId,
        PaginationQuery pagination,
        CancellationToken token = default
    )
    {
        var archives = await _gameArchiveRepository.GetPaginatedArchivedGamesForUserAsync(
            userId,
            pagination,
            token
        );
        var totalCount = await _gameArchiveRepository.CountArchivedGamesForUserAsync(userId, token);

        var summeries = archives.Select(CreateGameSummary);
        return new(
            Items: summeries,
            TotalCount: totalCount,
            Page: pagination.Page,
            PageSize: pagination.PageSize
        );
    }

    private static GameSummaryDto CreateGameSummary(GameArchive archive) =>
        new(
            archive.GameToken,
            new PlayerSummaryDto(
                UserId: archive.WhitePlayer.UserId,
                UserName: archive.WhitePlayer.UserName
            ),
            new PlayerSummaryDto(
                UserId: archive.BlackPlayer.UserId,
                UserName: archive.BlackPlayer.UserName
            ),
            PoolType: archive.PoolType,
            BaseSeconds: archive.BaseSeconds,
            IncrementSeconds: archive.IncrementSeconds,
            Result: archive.Result,
            CreatedAt: archive.CreatedAt
        );

    private static PlayerArchive CreatePlayerArchive(GamePlayer player) =>
        new()
        {
            Color = player.Color,
            UserId = player.UserId,
            UserName = player.UserName,
        };
}
