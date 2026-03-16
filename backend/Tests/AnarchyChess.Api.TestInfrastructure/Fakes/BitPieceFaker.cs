using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class BitPieceFaker : StructFaker<BitPiece>
{
    public BitPieceFaker(PieceType? pieceType = null, BitPieceColor? color = null)
    {
        StrictMode(true);
        RuleFor(x => x.Type, f => pieceType ?? f.PickRandom<PieceType>());
        RuleFor(x => x.Color, f => color ?? f.PickRandom<BitPieceColor>());
    }
}
