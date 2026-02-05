using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitMoveTests
{
    [Fact]
    public void AddCapture_packs_data_correctly()
    {
        BitMove move = new()
        {
            From = 10,
            To = 20,
            Piece = PieceType.Queen,
        };

        move.AddCapture(5, PieceType.Pawn, BitPieceColor.Black);

        var capture = move.GetCapture(0);
        capture.Position.Should().Be(5);
        capture.PieceType.Should().Be(PieceType.Pawn);
        capture.Color.Should().Be(BitPieceColor.Black);

        move.CapturesMask.Should().Be(UInt128.One << 5);

        move.CaptureCount.Should().Be(1);
    }

    [Fact]
    public void AddCapture_multiple_captures_are_stored_correctly()
    {
        BitMove move = new()
        {
            From = 1,
            To = 2,
            Piece = PieceType.Rook,
        };

        move.AddCapture(2, PieceType.Pawn, BitPieceColor.White);
        move.AddCapture(63, PieceType.King, BitPieceColor.Black);

        (byte Position, PieceType PieceType, BitPieceColor Color)[] expected =
        [
            (Position: 2, PieceType: PieceType.Pawn, Color: BitPieceColor.White),
            (Position: 63, PieceType: PieceType.King, Color: BitPieceColor.Black),
        ];

        for (int i = 0; i < move.CaptureCount; i++)
        {
            move.GetCapture(i).Should().BeEquivalentTo(expected[i]);
        }

        move.CapturesMask.Should().Be((UInt128.One << 2) | (UInt128.One << 63));
    }

    [Fact]
    public void AddCapture_throws_when_exceeding_max_captures()
    {
        BitMove move = new()
        {
            From = 1,
            To = 2,
            Piece = PieceType.Rook,
        };

        for (byte i = 0; i < 16; i++)
        {
            move.AddCapture(i, PieceType.Pawn, BitPieceColor.White);
        }

        Action act = () => move.AddCapture(16, PieceType.Pawn, BitPieceColor.White);
        act.Should().Throw<InvalidOperationException>().WithMessage("Too many captures");
    }

    [Fact]
    public void GetCapture_throws_when_index_out_of_range()
    {
        BitMove move = new()
        {
            From = 1,
            To = 2,
            Piece = PieceType.Rook,
        };

        move.AddCapture(0, PieceType.Pawn, BitPieceColor.White);

        Action act = () => move.GetCapture(1);
        act.Should().Throw<IndexOutOfRangeException>();
    }
}
