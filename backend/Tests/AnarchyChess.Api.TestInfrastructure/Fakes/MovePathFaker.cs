using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.TestData;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class MovePathFaker : RecordFaker<MovePath>
{
    public MovePathFaker()
    {
        StrictMode(true);
        RuleFor(x => x.FromIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.ToIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.MoveKey, f => (MoveKey)f.Random.String2(10));
        RuleFor(x => x.CapturedIdxs, GameTestData.RandomIdxs);
        RuleFor(x => x.TriggerIdxs, GameTestData.RandomIdxs);
        RuleFor(
            x => x.IntermediateSquares,
            f => new IntermediateSquarePathFaker().Generate(f.Random.Number(1, 5))
        );
        RuleFor(
            x => x.SideEffects,
            f => new MoveSideEffectPathFaker().Generate(f.Random.Number(1, 5))
        );
        RuleFor(x => x.PieceSpawns, f => new PieceSpawnPathFaker().Generate(f.Random.Number(1, 5)));
        RuleFor(x => x.PromotesTo, f => f.PickRandom<PieceType>());
        RuleFor(x => x.SpecialType, f => f.PickRandom<SpecialMoveType>());
        RuleFor(x => x.ForcedPriority, f => f.PickRandom<ForcedMovePriority>());
        RuleFor(x => x.EmphasizeSquare, f => f.Random.Bool());
        RuleFor(x => x.OvertimeRemovalIdxs, GameTestData.RandomIdxs);
    }
}
