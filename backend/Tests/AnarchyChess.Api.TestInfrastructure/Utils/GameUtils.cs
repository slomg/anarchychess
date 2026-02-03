using AnarchyChess.Api.Game.GameHandlers;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.UserRating.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public record StartGameResult(
    AuthedUser User1,
    CurrentRating User1Rating,
    AuthedUser User2,
    CurrentRating User2Rating,
    GameToken GameToken,
    PoolKey Pool
);

public static class GameUtils
{
    public static async Task<StartGameResult> CreateRatedGameAsync(
        ApplicationDbContext dbContext,
        IGameStarter gameStarter
    )
    {
        var user1 = new AuthedUserFaker().Generate();
        var user1Rating = new CurrentRatingFaker(user1, 1200)
            .RuleFor(x => x.TimeControl, TimeControl.Bullet)
            .Generate();

        var user2 = new AuthedUserFaker().Generate();
        var user2Rating = new CurrentRatingFaker(user2, 1300)
            .RuleFor(x => x.TimeControl, TimeControl.Bullet)
            .Generate();

        await dbContext.AddRangeAsync(user1, user1Rating, user2, user2Rating);
        await dbContext.SaveChangesAsync();

        TimeControlSettings timeControl = new(30, 0);
        PoolKey pool = new(PoolType.Rated, timeControl);
        var gameToken = await gameStarter.StartGameWithRandomColorsAsync(user1.Id, user2.Id, pool);

        return new(user1, user1Rating, user2, user2Rating, gameToken, pool);
    }

    public static GameData CreateGameData(
        IGameCore gameCore,
        IGameClock clocks,
        TimeControlSettings? timeControl = null,
        ChessBoard? board = null
    )
    {
        var whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
        var blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();
        var pool = new PoolKeyFaker().Generate();
        if (timeControl is not null)
        {
            pool = pool with { TimeControl = timeControl };
        }

        GameCoreState coreState;
        if (board is null)
        {
            coreState = new();
        }
        else
        {
            coreState = new() { Board = board };
        }

        return new()
        {
            Players = new PlayerRoster(whitePlayer, blackPlayer),
            GameSource = GameSource.Matchmaking,
            Pool = pool,
            InitialFen = gameCore.StartGame(coreState).FullFen,
            Core = coreState,
            ClockState = clocks.Create(pool.TimeControl),
        };
    }

    public static Move GetLegalMove(IGameCore gameCore, GameData gameData) =>
        gameCore.GetLegalMoves(gameData.Core).MoveMap.First().Value;

    public static async Task GoOutOfGracePeriodAsync(
        IMoveHandler moveHandler,
        IGameCore gameCore,
        GameToken gameToken,
        GameData gameData
    )
    {
        await moveHandler.HandleMoveAsync(
            gameData.Players.WhitePlayer.UserId,
            new MoveKey(GetLegalMove(gameCore, gameData)),
            gameToken,
            gameData
        );
        await moveHandler.HandleMoveAsync(
            gameData.Players.BlackPlayer.UserId,
            new MoveKey(GetLegalMove(gameCore, gameData)),
            gameToken,
            gameData
        );
    }
}
