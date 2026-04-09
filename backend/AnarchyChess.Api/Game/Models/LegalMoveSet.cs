using AnarchyChess.Ai.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

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

    public Move? FindBotMove(BitMove botMove) =>
        AllMoves.FirstOrDefault(move =>
        {
            UInt128 moveCaptureMask = 0;
            foreach (var capture in move.Captures)
            {
                moveCaptureMask |= UInt128.One << capture.Position.AsIdx();
            }

            if (move.From.AsIdx() == botMove.From && move.To.AsIdx() == botMove.To)
            {
                Console.WriteLine("");
            }

            return move.From.AsIdx() == botMove.From
                && move.To.AsIdx() == botMove.To
                && move.PromotesTo == botMove.PromotesTo
                && moveCaptureMask == botMove.CapturesMask
                && move.SpecialMoveType == botMove.SpecialMoveType;
        });
}
