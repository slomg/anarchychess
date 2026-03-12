using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class BitPieceFaker : StructFaker<BitPiece>
{
    public BitPieceFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Type, f => f.PickRandom<PieceType>());
        RuleFor(x => x.Color, f => f.PickRandom<BitPieceColor>());
    }
}
