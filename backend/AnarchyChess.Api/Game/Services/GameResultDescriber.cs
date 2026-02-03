using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.Services;

public interface IGameResultDescriber
{
    GameEndStatus KingSelfCapture(GameColor by);
    GameEndStatus KingCaptured(GameColor by);
    GameEndStatus Resignation(GameColor by);
    GameEndStatus Abandoned(GameColor by);
    GameEndStatus Overtime(GameColor by);
    GameEndStatus Aborted(GameColor by);

    GameEndStatus DrawByAgreement();
    GameEndStatus FiftyMoves();
    GameEndStatus ThreeFold();
    GameEndStatus KingTouch();
    GameEndStatus MutualKingCapture();
}

public class GameResultDescriber : IGameResultDescriber
{
    public GameEndStatus KingCaptured(GameColor by) =>
        new(GetResultByWinner(by), $"{by} Captured the King");

    public GameEndStatus KingSelfCapture(GameColor by) =>
        new(GetResultByLoser(by), $"{by} Captured Their Own King");

    public GameEndStatus Aborted(GameColor by) => new(GameResult.Aborted, $"Game Aborted by {by}");

    public GameEndStatus Resignation(GameColor by) =>
        new(GetResultByLoser(by), $"{by.Invert()} Won by Resignation");

    public GameEndStatus Abandoned(GameColor by) =>
        new(GetResultByLoser(by), $"{by} Abandoned the Game");

    public GameEndStatus Overtime(GameColor by) =>
        new(GetResultByLoser(by), $"{by}'s King Got Bored and Left");

    public GameEndStatus ThreeFold() => new(GameResult.Draw, "Draw by 3-Fold Repetition");

    public GameEndStatus FiftyMoves() => new(GameResult.Draw, "Draw by 50 Moves Rule");

    public GameEndStatus DrawByAgreement() => new(GameResult.Draw, "Draw by Agreement");

    public GameEndStatus KingTouch() => new(GameResult.Draw, "Draw by King Touch");

    public GameEndStatus MutualKingCapture() => new(GameResult.Draw, "Draw by Mutual King Capture");

    private static GameResult GetResultByLoser(GameColor loser) =>
        loser.Match(whenWhite: GameResult.BlackWin, whenBlack: GameResult.WhiteWin);

    private static GameResult GetResultByWinner(GameColor winner) =>
        winner.Match(whenWhite: GameResult.WhiteWin, whenBlack: GameResult.BlackWin);
}
