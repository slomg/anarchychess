using AnarchyChess.Ai.Helpers;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitboardHelpersTests
{
    [Fact]
    public void BitScanForward_returns_the_index_of_the_least_significant_bit()
    {
        UInt128 bitboard = 0b10010;
        int index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(1);
        bitboard.Should().Be(0b10000);
    }

    [Fact]
    public void BitScanForward_handles_a_single_bit_set()
    {
        UInt128 bitboard = 0b10000000;
        int index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(7);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanForward_handles_high_bit_set()
    {
        UInt128 bitboard = UInt128.One << 67;
        int index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(67);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanForward_throws_for_an_empty_bitboard()
    {
        UInt128 bitboard = 0;

        Action act = () => BitboardHelpers.BitScanForward(ref bitboard);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MaskAdjacent_sets_correct_adjacent_bits_in_middle()
    {
        byte position = 21; // somewhere in the middle of the board
        UInt128 result = BitboardHelpers.MaskAdjacent(position);

        UInt128 expected =
            (UInt128.One << 20)
            | // left
            (UInt128.One << 22)
            | // right
            (UInt128.One << 11)
            | // up
            (UInt128.One << 31)
            | // down
            (UInt128.One << 10)
            | // up left
            (UInt128.One << 12)
            | // up right
            (UInt128.One << 30)
            | // down left
            (UInt128.One << 32); // down right

        result.Should().Be(expected);
    }

    [Fact]
    public void MaskAdjacent_does_not_wrap_around_edges()
    {
        byte position = 0;
        UInt128 result = BitboardHelpers.MaskAdjacent(position);

        UInt128 expected =
            (UInt128.One << 1)
            | // right
            (UInt128.One << 10)
            | // up
            (UInt128.One << 11); // up-right

        result.Should().Be(expected);
    }

    [Fact]
    public void CreateMoveFromAttacks_handles_mixed_quiet_and_capture()
    {
        var capturedPiece = PieceFactory.White();
        BitBoard board = BitBoard.FromPieces(new() { [new("c1")] = capturedPiece });

        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        PieceType piece = PieceType.Rook;
        UInt128 attacks = (UInt128.One << 1) | (UInt128.One << 2);

        BitboardHelpers.CreateMoveFromAttacks(
            from,
            piece,
            board,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );

        BitMove quietMove = new()
        {
            From = 0,
            To = 1,
            Piece = piece,
        };
        BitMove captureMove = new()
        {
            From = 0,
            To = 2,
            Piece = piece,
        };
        captureMove.AddCapture(square: 2, piece: capturedPiece.Type, color: BitPieceColor.White);

        List<BitMove> expectedMoves = [quietMove, captureMove];
        List<BitMove> result = [.. moves[..moveCount]];

        result.Should().BeEquivalentTo(expectedMoves);
    }
}
