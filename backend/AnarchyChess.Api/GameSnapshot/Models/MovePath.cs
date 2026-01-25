using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MovePath")]
public record MovePath(
    [property: Id(0)] byte FromIdx,
    [property: Id(1)] byte ToIdx,
    [property: Id(2)] MoveKey MoveKey,
    [property: Id(3)] IReadOnlyCollection<byte>? CapturedIdxs,
    [property: Id(4)] IReadOnlyCollection<byte>? TriggerIdxs,
    [property: Id(5)] IReadOnlyCollection<IntermediateSquarePath>? IntermediateSquares,
    [property: Id(6)] IReadOnlyList<MoveSideEffectPath>? SideEffects,
    [property: Id(7)] IReadOnlyList<PieceSpawnPath>? PieceSpawns,
    [property: Id(8)] PieceType? PromotesTo,
    [property: Id(9)] SpecialMoveType? SpecialType,
    [property: Id(10)] ForcedMovePriority? ForcedPriority,
    [property: Id(11)] bool? EmphasizeSquare,
    [property: Id(12)] IReadOnlyCollection<byte>? OvertimeRemovalIdxs
)
{
    public static MovePath FromMove(Move move, int boardWidth, MoveKey? moveKey = null)
    {
        var captures =
            move.Captures.Count != 0
                ? move.Captures.Select(c => c.Position.AsIndex(boardWidth)).ToList()
                : null;
        var triggers =
            move.TriggerSquares.Count != 0
                ? move.TriggerSquares.Select(t => t.AsIndex(boardWidth)).ToList()
                : null;
        var intermediates =
            move.IntermediateSquares.Count != 0
                ? move
                    .IntermediateSquares.Select(i =>
                        IntermediateSquarePath.FromIntermediateSquare(i, boardWidth)
                    )
                    .ToList()
                : null;
        var sideEffects =
            move.SideEffects.Count != 0
                ? move
                    .SideEffects.Select(m => MoveSideEffectPath.FromMoveSideEffect(m, boardWidth))
                    .ToList()
                : null;
        var spawns =
            move.PieceSpawns.Count != 0
                ? move
                    .PieceSpawns.Select(p => PieceSpawnPath.FromPieceSpawn(p, boardWidth))
                    .ToList()
                : null;

        return new(
            FromIdx: move.From.AsIndex(boardWidth),
            ToIdx: move.To.AsIndex(boardWidth),
            MoveKey: moveKey ?? new MoveKey(move),
            CapturedIdxs: captures,
            TriggerIdxs: triggers,
            IntermediateSquares: intermediates,
            SideEffects: sideEffects,
            PromotesTo: move.PromotesTo,
            PieceSpawns: spawns,
            SpecialType: move.SpecialMoveType is SpecialMoveType.None ? null : move.SpecialMoveType,
            ForcedPriority: move.ForcedPriority is ForcedMovePriority.None
                ? null
                : move.ForcedPriority,
            EmphasizeSquare: move.EmphasizeSquare ? true : null,
            OvertimeRemovalIdxs: null
        );
    }
}
