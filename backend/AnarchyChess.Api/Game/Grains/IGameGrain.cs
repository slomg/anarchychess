using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using ErrorOr;

namespace AnarchyChess.Api.Game.Grains;

[Alias("AnarchyChess.Api.Game.Grains.IGameGrain")]
public interface IGameGrain : IGrainWithStringKey
{
    [Alias("StartGameAsync")]
    Task StartGameAsync(
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        PoolKey pool,
        GameSource gameSource,
        CancellationToken token = default
    );

    [Alias("SyncRevisionAsync")]
    Task<ErrorOr<Success>> SyncRevisionAsync(
        ConnectionId connectionId,
        CancellationToken token = default
    );

    [Alias("GetStateAsync")]
    Task<ErrorOr<GameState>> GetStateAsync();

    [Alias("IsGameOngoingAsync")]
    Task<bool> IsGameOngoingAsync();

    [Alias("GetPlayersAsync")]
    Task<ErrorOr<PlayerRoster>> GetPlayersAsync();

    [Alias("GetMovesAsync")]
    Task<ErrorOr<IReadOnlyList<Move>>> GetMovesAsync();

    [Alias("RequestGameEndAsync")]
    Task<ErrorOr<Success>> RequestGameEndAsync(UserId byUserId, CancellationToken token = default);

    [Alias("RequestDrawAsync")]
    Task<ErrorOr<Success>> RequestDrawAsync(UserId byUserId, CancellationToken token = default);

    [Alias("DeclineDrawAsync")]
    Task<ErrorOr<Success>> DeclineDrawAsync(UserId byUserId, CancellationToken token = default);

    [Alias("MovePieceAsync")]
    Task<ErrorOr<Success>> MovePieceAsync(
        UserId byUserId,
        MoveKey key,
        CancellationToken token = default
    );
}
