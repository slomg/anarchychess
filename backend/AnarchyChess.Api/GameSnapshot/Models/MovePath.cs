using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MovePath")]
public record MovePath(
    [property: Id(0)] byte FromIdx,
    [property: Id(1)] byte ToIdx,
    [property: Id(2)] MoveKey MoveKey,
    [property: Id(3)] IReadOnlyCollection<byte>? CapturedIdxs = null,
    [property: Id(4)] IReadOnlyCollection<byte>? TriggerIdxs = null,
    [property: Id(5)] IReadOnlyCollection<IntermediateSquarePath>? IntermediateSquares = null,
    [property: Id(6)] IReadOnlyList<MoveSideEffectPath>? SideEffects = null,
    [property: Id(7)] IReadOnlyList<PieceSpawnPath>? PieceSpawns = null,
    [property: Id(13)] IReadOnlyCollection<MoveStunPath>? Stuns = null,
    [property: Id(8)] PieceType? PromotesTo = null,
    [property: Id(9)] SpecialMoveType? SpecialType = null,
    [property: Id(10)] ForcedMovePriority? ForcedPriority = null,
    [property: Id(11)] bool? EmphasizeSquare = null,
    [property: Id(12)] IReadOnlyCollection<byte>? OvertimeRemovalIdxs = null
)
{
    public static MovePath FromMove(Move move, int boardWidth, MoveKey? moveKey = null)
    {
        var captures =
            move.Captures.Count > 0
                ? move.Captures.Select(x => x.Position.AsIdx(boardWidth)).ToList()
                : null;
        var triggers =
            move.TriggerSquares.Count > 0
                ? move.TriggerSquares.Select(x => x.AsIdx(boardWidth)).ToList()
                : null;
        var intermediates =
            move.IntermediateSquares.Count > 0
                ? move
                    .IntermediateSquares.Select(x =>
                        IntermediateSquarePath.FromIntermediateSquare(x, boardWidth)
                    )
                    .ToList()
                : null;
        var sideEffects =
            move.SideEffects.Count > 0
                ? move
                    .SideEffects.Select(x => MoveSideEffectPath.FromMoveSideEffect(x, boardWidth))
                    .ToList()
                : null;
        var spawns =
            move.PieceSpawns.Count > 0
                ? move
                    .PieceSpawns.Select(x => PieceSpawnPath.FromPieceSpawn(x, boardWidth))
                    .ToList()
                : null;
        var stuns =
            move.Stuns.Count > 0
                ? move.Stuns.Select(x => MoveStunPath.FromMoveStun(x, boardWidth)).ToList()
                : null;

        return new(
            FromIdx: move.From.AsIdx(boardWidth),
            ToIdx: move.To.AsIdx(boardWidth),
            MoveKey: moveKey ?? new MoveKey(move),
            CapturedIdxs: captures,
            TriggerIdxs: triggers,
            IntermediateSquares: intermediates,
            SideEffects: sideEffects,
            PromotesTo: move.PromotesTo,
            PieceSpawns: spawns,
            Stuns: stuns,
            SpecialType: move.SpecialMoveType is SpecialMoveType.None ? null : move.SpecialMoveType,
            ForcedPriority: move.ForcedPriority is ForcedMovePriority.None
                ? null
                : move.ForcedPriority,
            EmphasizeSquare: move.EmphasizeSquare ? true : null,
            OvertimeRemovalIdxs: null
        );
    }
}
