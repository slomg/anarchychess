using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class BotGameStateFaker : RecordFaker<BotGameState>
{
    public BotGameStateFaker()
    {
        StrictMode(true);
        RuleFor(x => x.WhitePlayer, f => new GamePlayerFaker(GameColor.White).Generate());
        RuleFor(x => x.BlackPlayer, f => new GamePlayerFaker(GameColor.Black).Generate());
        RuleFor(x => x.BotColor, f => f.PickRandom<GameColor>());
        RuleFor(x => x.BotType, f => f.PickRandom<BotType>());
        RuleFor(x => x.SideToMove, f => f.PickRandom<GameColor>());
        RuleFor(x => x.InitialFen, "10/10/10/10/10/10/10/10/10/10");
        RuleFor(x => x.MoveHistory, f => new MoveSnapshotFaker().Generate(f.Random.Number(1, 6)));
        RuleFor(x => x.LegalMoves, f => new MovePathFaker().Generate(f.Random.Number(1, 10)));
        RuleFor(x => x.ResultData, (GameResultData?)null);
    }
}
