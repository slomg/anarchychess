using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class MoveUndoStateTests
{
    private static MoveUndoState CreateUndoState() =>
        new()
        {
            From = 10,
            To = 20,
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            PromotedTo = null,
            SpecialMoveType = SpecialMoveType.None,
            HasMoved = 0,
            StunnedPieces = 0,
            EnPassantSquaresMask = 0,
            EnPassantPawnSquare = 0,
            IsWhiteToMove = true,
            LastCaptureMask = 0,

            WhiteMaterialCount = 0,
            BlackMaterialCount = 0,
        };

    [Fact]
    public void AddCapture_packs_data_correctly()
    {
        var undo = CreateUndoState();

        undo.AddCapture(5, PieceType.Pawn, BitPieceColor.Black);

        var capture = undo.GetCapture(0);
        capture.Position.Should().Be(5);
        capture.PieceType.Should().Be(PieceType.Pawn);
        capture.Color.Should().Be(BitPieceColor.Black);

        undo.CaptureCount.Should().Be(1);
    }

    [Fact]
    public void AddCapture_multiple_captures_are_stored_correctly()
    {
        var undo = CreateUndoState();

        undo.AddCapture(2, PieceType.Pawn, BitPieceColor.White);
        undo.AddCapture(63, PieceType.King, BitPieceColor.Black);

        (byte Position, PieceType PieceType, BitPieceColor Color)[] expected =
        [
            (Position: 2, PieceType: PieceType.Pawn, Color: BitPieceColor.White),
            (Position: 63, PieceType: PieceType.King, Color: BitPieceColor.Black),
        ];

        for (int i = 0; i < undo.CaptureCount; i++)
        {
            undo.GetCapture(i).Should().BeEquivalentTo(expected[i]);
        }
    }

    [Fact]
    public void AddCapture_throws_when_exceeding_max_captures()
    {
        var undo = CreateUndoState();

        for (byte i = 0; i < 16; i++)
        {
            undo.AddCapture(i, PieceType.Pawn, BitPieceColor.White);
        }

        Action act = () => undo.AddCapture(16, PieceType.Pawn, BitPieceColor.White);
        act.Should().Throw<InvalidOperationException>().WithMessage("Too many captures");
    }

    [Fact]
    public void GetCapture_throws_when_index_out_of_range()
    {
        var undo = CreateUndoState();

        undo.AddCapture(0, PieceType.Pawn, BitPieceColor.White);

        Action act = () => undo.GetCapture(1);
        act.Should().Throw<IndexOutOfRangeException>();
    }
}
