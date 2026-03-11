using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.LegalMoveSet")]
public record LegalMoveSet(
    IReadOnlyDictionary<MoveKey, Move> MoveMap,
    IReadOnlyCollection<MovePath> MovePaths
)
{
    public IEnumerable<Move> AllMoves => MoveMap.Values;

    public LegalMoveSet()
        : this(MoveMap: new Dictionary<MoveKey, Move>(), MovePaths: []) { }

    public Move? FindBotMove(AiEngineMove botMove) =>
        AllMoves.FirstOrDefault(move =>
        {
            HashSet<AlgebraicPoint> moveCaptures = [.. move.Captures.Select(c => c.Position)];
            HashSet<AlgebraicPoint> botCaptures = botMove.Captures?.ToHashSet() ?? [];

            return move.From == botMove.From
                && move.To == botMove.To
                && move.PromotesTo == botMove.PromotesTo
                && moveCaptures.SetEquals(botCaptures);
        });
}
