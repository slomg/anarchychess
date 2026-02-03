using AnarchyChess.Ai.Constants;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitPieceMapTests
{
    [Fact]
    public void Colored_maps_all_piece_types_correctly()
    {
        var expected = new Dictionary<PieceType, BitPiece>
        {
            [PieceType.King] = BitPiece.King,
            [PieceType.Queen] = BitPiece.Queen,
            [PieceType.Pawn] = BitPiece.Pawn,
            [PieceType.Rook] = BitPiece.Rook,
            [PieceType.Bishop] = BitPiece.Bishop,
            [PieceType.Horsey] = BitPiece.Horsey,
            [PieceType.Knook] = BitPiece.Knook,
            [PieceType.Antiqueen] = BitPiece.Antiqueen,
            [PieceType.UnderagePawn] = BitPiece.UnderagePawn,
            [PieceType.SterilePawn] = BitPiece.SterilePawn,
            [PieceType.Checker] = BitPiece.Checker,
        };

        BitPieceMap.Colored.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Colored_has_expected_length()
    {
        BitPieceMap.Colored.Count.Should().Be(Enum.GetValues<BitPiece>().Length);
    }

    [Fact]
    public void Neutral_maps_all_piece_types_correctly()
    {
        var expected = new Dictionary<PieceType, NeutralBitPiece>
        {
            [PieceType.TraitorRook] = NeutralBitPiece.TraitorRook,
        };

        BitPieceMap.Neutral.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Neutral_has_expected_length()
    {
        BitPieceMap.Neutral.Count.Should().Be(Enum.GetValues<NeutralBitPiece>().Length);
    }
}
