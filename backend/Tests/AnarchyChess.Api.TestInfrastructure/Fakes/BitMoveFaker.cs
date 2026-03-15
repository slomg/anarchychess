using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class BitMoveFaker : StructFaker<BitMove>
{
    public BitMoveFaker(
        PieceType? pieceType = null,
        AlgebraicPoint? from = null,
        AlgebraicPoint? to = null
    )
    {
        StrictMode(true);
        RuleFor(x => x.From, f => from?.AsIdx() ?? f.Random.Number(min: 0, max: 100));
        RuleFor(x => x.To, f => to?.AsIdx() ?? f.Random.Number(min: 0, max: 100));
        RuleFor(x => x.Piece, f => new BitPieceFaker(pieceType).Generate());
        RuleFor(x => x.CapturesMask, UInt128.Zero);
        RuleFor(x => x.PromotesTo, (PieceType?)null);
        RuleFor(x => x.ForcedMovePriority, ForcedMovePriority.None);
        RuleFor(x => x.SpecialMoveType, SpecialMoveType.None);
    }
}
