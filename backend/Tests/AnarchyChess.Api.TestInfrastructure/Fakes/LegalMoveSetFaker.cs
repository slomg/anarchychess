using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class LegalMoveSetFaker : RecordFaker<LegalMoveSet>
{
    public LegalMoveSetFaker()
    {
        StrictMode(true);
        RuleFor(x => x.MoveMap, new Dictionary<MoveKey, Move>());
        RuleFor(x => x.MovePaths, f => new MovePathFaker().Generate(5));
    }
}
