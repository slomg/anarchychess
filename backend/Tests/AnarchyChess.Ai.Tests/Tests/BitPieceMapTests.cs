using AnarchyChess.Ai.Constants;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitPieceMapTests
{
    [Fact]
    public void Colored_maps_all_piece_types_correctly()
    {
        var expected = new Dictionary<PieceType, BitPieceType>
        {
            [PieceType.King] = BitPieceType.King,
            [PieceType.Queen] = BitPieceType.Queen,
            [PieceType.Pawn] = BitPieceType.Pawn,
            [PieceType.Rook] = BitPieceType.Rook,
            [PieceType.Bishop] = BitPieceType.Bishop,
            [PieceType.Horsey] = BitPieceType.Horsey,
            [PieceType.Knook] = BitPieceType.Knook,
            [PieceType.Antiqueen] = BitPieceType.Antiqueen,
            [PieceType.UnderagePawn] = BitPieceType.UnderagePawn,
            [PieceType.SterilePawn] = BitPieceType.SterilePawn,
            [PieceType.Checker] = BitPieceType.Checker,
        };

        BitPieceMap.Colored.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Colored_has_expected_length()
    {
        BitPieceMap.Colored.Count.Should().Be(Enum.GetValues<BitPieceType>().Length);
    }

    [Fact]
    public void Neutral_maps_all_piece_types_correctly()
    {
        var expected = new Dictionary<PieceType, NeutralBitPieceType>
        {
            [PieceType.TraitorRook] = NeutralBitPieceType.TraitorRook,
        };

        BitPieceMap.Neutral.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Neutral_has_expected_length()
    {
        BitPieceMap.Neutral.Count.Should().Be(Enum.GetValues<NeutralBitPieceType>().Length);
    }
}
