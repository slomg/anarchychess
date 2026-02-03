using AnarchyChess.Ai.Constants;
using AnarchyChess.Ai.Extensions;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitPieceMapTests
{
    [Fact]
    public void PieceTypeToBitPiece_maps_all_colored_piece_types_correctly()
    {
        var expected = new Dictionary<(PieceType, GameColor?), BitPieceType>
        {
            // White pieces
            [(PieceType.King, GameColor.White)] = BitPieceType.WhiteKing,
            [(PieceType.Queen, GameColor.White)] = BitPieceType.WhiteQueen,
            [(PieceType.Pawn, GameColor.White)] = BitPieceType.WhitePawn,
            [(PieceType.Rook, GameColor.White)] = BitPieceType.WhiteRook,
            [(PieceType.Bishop, GameColor.White)] = BitPieceType.WhiteBishop,
            [(PieceType.Horsey, GameColor.White)] = BitPieceType.WhiteHorsey,
            [(PieceType.Knook, GameColor.White)] = BitPieceType.WhiteKnook,
            [(PieceType.Antiqueen, GameColor.White)] = BitPieceType.WhiteAntiqueen,
            [(PieceType.UnderagePawn, GameColor.White)] = BitPieceType.WhiteUnderagePawn,
            [(PieceType.SterilePawn, GameColor.White)] = BitPieceType.WhiteSterilePawn,
            [(PieceType.Checker, GameColor.White)] = BitPieceType.WhiteChecker,

            // Black pieces
            [(PieceType.King, GameColor.Black)] = BitPieceType.BlackKing,
            [(PieceType.Queen, GameColor.Black)] = BitPieceType.BlackQueen,
            [(PieceType.Pawn, GameColor.Black)] = BitPieceType.BlackPawn,
            [(PieceType.Rook, GameColor.Black)] = BitPieceType.BlackRook,
            [(PieceType.Bishop, GameColor.Black)] = BitPieceType.BlackBishop,
            [(PieceType.Horsey, GameColor.Black)] = BitPieceType.BlackHorsey,
            [(PieceType.Knook, GameColor.Black)] = BitPieceType.BlackKnook,
            [(PieceType.Antiqueen, GameColor.Black)] = BitPieceType.BlackAntiqueen,
            [(PieceType.UnderagePawn, GameColor.Black)] = BitPieceType.BlackUnderagePawn,
            [(PieceType.SterilePawn, GameColor.Black)] = BitPieceType.BlackSterilePawn,
            [(PieceType.Checker, GameColor.Black)] = BitPieceType.BlackChecker,

            // Neutral pieces
            [(PieceType.TraitorRook, null)] = BitPieceType.TraitorRook,
        };

        BitPieceMap.PieceTypeToBitPieceType.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void PieceTypeToBitPiece_has_expected_count()
    {
        int whiteCount = Enum.GetValues<BitPieceType>()
            .Cast<BitPieceType>()
            .Count(p => p.IsWhite());
        int blackCount = Enum.GetValues<BitPieceType>()
            .Cast<BitPieceType>()
            .Count(p => p.IsBlack());
        int neutralCount = Enum.GetValues<BitPieceType>()
            .Cast<BitPieceType>()
            .Count(p => p.IsNeutral());

        int expectedCount = whiteCount + blackCount + neutralCount;
        BitPieceMap.PieceTypeToBitPieceType.Count.Should().Be(expectedCount);
    }

    [Fact]
    public void FromPiece_returns_expected_BitPieceType()
    {
        BitPieceMap.FromPiece(PieceType.King, GameColor.White).Should().Be(BitPieceType.WhiteKing);
        BitPieceMap
            .FromPiece(PieceType.Bishop, GameColor.Black)
            .Should()
            .Be(BitPieceType.BlackBishop);
        BitPieceMap.FromPiece(PieceType.TraitorRook, null).Should().Be(BitPieceType.TraitorRook);
    }
}
