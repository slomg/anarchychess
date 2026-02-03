using System.Text.Json.Serialization;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.GameLogic.Models;

/// <param name="From">Origin square</param>
/// <param name="To">Destination square</param>
/// <param name="Piece">The piece being moved</param>
/// <param name="TriggerSquares">
/// Squares (other than the destination) that the user can click on to trigger this move.
/// </param>
/// <param name="IntermediateSquares">
/// Squares the piece passes through before reaching its final destination.
/// Used for frontend animation
/// </param>
/// <param name="Captures">Information about what pieces were captured and where</param>
/// <param name="SideEffects">Any additional movement caused by this</param>
/// <param name="SpecialMoveType">Indicates the type of move</param>
/// <param name="PieceSpawns">Any new pieces to spawn</param>
/// <param name="ForcedPriority">
/// Indicates whether this move is forced, and if so, how strongly it should be prioritized.
/// The move(s) with the highest <see cref="ForcedMovePriority"/> will be the only ones allowed
/// </param>
/// <param name="PromotesTo">
/// What piece to promote to if any. Other properties of <see cref="Piece"/> will remain unchanged
/// </param>
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.Move")]
[method: JsonConstructor]
public record Move(
    AlgebraicPoint From,
    AlgebraicPoint To,
    Piece Piece,
    IReadOnlyCollection<AlgebraicPoint> TriggerSquares,
    IReadOnlyCollection<IntermediateSquare> IntermediateSquares,
    IReadOnlyCollection<MoveCapture> Captures,
    IReadOnlyCollection<MoveSideEffect> SideEffects,
    IReadOnlyCollection<PieceSpawn> PieceSpawns,
    SpecialMoveType SpecialMoveType,
    ForcedMovePriority ForcedPriority,
    PieceType? PromotesTo,
    bool EmphasizeSquare
)
{
    public Move(
        AlgebraicPoint from,
        AlgebraicPoint to,
        Piece piece,
        IEnumerable<AlgebraicPoint>? triggerSquares = null,
        IEnumerable<IntermediateSquare>? intermediateSquares = null,
        IEnumerable<MoveCapture>? captures = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? pieceSpawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null,
        bool emphasizeSquare = false
    )
        : this(
            From: from,
            To: to,
            Piece: piece,
            TriggerSquares: triggerSquares?.ToList() ?? [],
            IntermediateSquares: intermediateSquares?.ToList() ?? [],
            Captures: captures?.ToList() ?? [],
            SideEffects: sideEffects?.ToList() ?? [],
            PieceSpawns: pieceSpawns?.ToList() ?? [],
            SpecialMoveType: specialMoveType,
            ForcedPriority: forcedPriority,
            PromotesTo: promotesTo,
            EmphasizeSquare: emphasizeSquare
        ) { }

    public IEnumerable<MoveStep> Flatten()
    {
        if (SideEffects != null)
        {
            foreach (var side in SideEffects)
            {
                yield return new(From: side.From, To: side.To, IsSelfCapture: side.From == side.To);
            }
        }

        bool isMainSelfCapture = From == To && PromotesTo is null;
        yield return new(From: From, To: To, IsSelfCapture: isMainSelfCapture);
    }
}

public readonly record struct MoveStep(AlgebraicPoint From, AlgebraicPoint To, bool IsSelfCapture);
