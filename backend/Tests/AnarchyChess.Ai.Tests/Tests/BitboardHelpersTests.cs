using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitboardHelpersTests
{
    [Fact]
    public void BitScanForward_returns_the_index_of_the_least_significant_bit()
    {
        UInt128 bitboard = 0b10010;
        byte index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(1);
        bitboard.Should().Be(0b10000);
    }

    [Fact]
    public void BitScanForward_handles_a_single_bit_set()
    {
        UInt128 bitboard = 0b10000000;
        byte index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(7);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanForward_handles_high_bit_set()
    {
        UInt128 bitboard = UInt128.One << 67;
        byte index = BitboardHelpers.BitScanForward(ref bitboard);

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
    public void BitScanBackward_returns_the_index_of_the_most_significant_bit()
    {
        UInt128 bitboard = 0b10010;
        byte index = BitboardHelpers.BitScanBackward(ref bitboard);

        index.Should().Be(4);
        bitboard.Should().Be(0b10);
    }

    [Fact]
    public void BitScanBackward_handles_a_single_bit_set()
    {
        UInt128 bitboard = 0b10000000;
        byte index = BitboardHelpers.BitScanBackward(ref bitboard);

        index.Should().Be(7);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanBackward_handles_high_bit_set()
    {
        UInt128 bitboard = UInt128.One << 67;
        byte index = BitboardHelpers.BitScanBackward(ref bitboard);

        index.Should().Be(67);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanBackward_throws_for_an_empty_bitboard()
    {
        UInt128 bitboard = 0;

        Action act = () => BitboardHelpers.BitScanBackward(ref bitboard);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0UL, 0UL, 0)] // empty
    [InlineData(1UL << 3, 0UL, 1)] // low half only
    [InlineData(0UL, 1UL << 5, 1)] // high half only
    [InlineData(1UL << 1, 1UL << 1, 2)] // both halves
    [InlineData(1UL << 63, 1UL << 0, 2)] // boundary correctness
    public void CountBits_adds_low_and_high_halves_correctly(ulong low, ulong high, int expected)
    {
        UInt128 mask = ((UInt128)high << 64) | low;

        BitboardHelpers.CountBits(mask).Should().Be(expected);
    }

    [Fact]
    public void ShiftRight_moves_bits_one_file_right()
    {
        UInt128 mask = UInt128.One << 11;

        UInt128 result = BitboardHelpers.ShiftRight(mask);

        result.Should().Be(UInt128.One << 12);
    }

    [Fact]
    public void ShiftRight_drops_bits_on_right_edge()
    {
        UInt128 result = BitboardHelpers.ShiftRight(BitboardConstants.RightEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void ShiftLeft_moves_bits_one_file_left()
    {
        UInt128 mask = UInt128.One << 11;

        UInt128 result = BitboardHelpers.ShiftLeft(mask);

        result.Should().Be(UInt128.One << 10);
    }

    [Fact]
    public void ShiftLeft_drops_bits_on_left_edge()
    {
        UInt128 result = BitboardHelpers.ShiftLeft(BitboardConstants.LeftEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void ShiftUp_moves_bits_up_one_rank()
    {
        UInt128 mask = UInt128.One << 12;

        UInt128 result = BitboardHelpers.ShiftUp(mask);

        result.Should().Be(UInt128.One << 22);
    }

    [Fact]
    public void ShiftUp_drops_bits_on_top_edge()
    {
        UInt128 result = BitboardHelpers.ShiftUp(BitboardConstants.TopEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void ShiftUpUnmasked_moves_bits_up_one_rank()
    {
        UInt128 mask = UInt128.One << 12;

        UInt128 result = BitboardHelpers.ShiftUpUnmasked(mask);

        result.Should().Be(UInt128.One << 22);
    }

    [Fact]
    public void ShiftUpUnmasked_keeps_top_edge_bits()
    {
        UInt128 result = BitboardHelpers.ShiftUpUnmasked(BitboardConstants.TopEdgeMask);

        result.Should().Be(BitboardConstants.TopEdgeMask << 10);
    }

    [Fact]
    public void ShiftDown_moves_bits_down_one_rank()
    {
        UInt128 mask = UInt128.One << 22;

        UInt128 result = BitboardHelpers.ShiftDown(mask);

        result.Should().Be(UInt128.One << 12);
    }

    [Fact]
    public void ShiftUpRight_moves_bits_diagonally()
    {
        UInt128 mask = UInt128.One << 12;

        UInt128 result = BitboardHelpers.ShiftUpRight(mask);

        result.Should().Be(UInt128.One << 23);
    }

    [Fact]
    public void ShiftUpRight_drops_bits_on_top_right_edge()
    {
        UInt128 result = BitboardHelpers.ShiftUpRight(BitboardConstants.TopRightEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void ShiftUpLeft_moves_bits_diagonally()
    {
        UInt128 mask = UInt128.One << 12;

        UInt128 result = BitboardHelpers.ShiftUpLeft(mask);

        result.Should().Be(UInt128.One << 21);
    }

    [Fact]
    public void ShiftUpLeft_drops_bits_on_top_left_edge()
    {
        UInt128 result = BitboardHelpers.ShiftUpLeft(BitboardConstants.TopLeftEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void ShiftDownRight_moves_bits_diagonally()
    {
        UInt128 mask = UInt128.One << 21;

        UInt128 result = BitboardHelpers.ShiftDownRight(mask);

        result.Should().Be(UInt128.One << 12);
    }

    [Fact]
    public void ShiftDownRight_drops_bits_on_bottom_right_edge()
    {
        UInt128 result = BitboardHelpers.ShiftDownRight(BitboardConstants.BottomRightEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void ShiftDownLeft_moves_bits_diagonally()
    {
        UInt128 mask = UInt128.One << 23;

        UInt128 result = BitboardHelpers.ShiftDownLeft(mask);

        result.Should().Be(UInt128.One << 12);
    }

    [Fact]
    public void ShiftDownLeft_drops_bits_on_bottom_left_edge()
    {
        UInt128 result = BitboardHelpers.ShiftDownLeft(BitboardConstants.BottomLeftEdgeMask);

        result.Should().Be(0);
    }

    [Fact]
    public void CreateMoveFromAttacks_handles_mixed_quiet_and_capture()
    {
        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        BitPiece piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White };
        UInt128 attacks = (UInt128.One << 1) | (UInt128.One << 2);
        UInt128 occupancy = UInt128.One | (UInt128.One << 2);

        BitboardHelpers.CreateMoveFromAttacks(
            from,
            piece,
            attacks: attacks,
            occupancy: occupancy,
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
            CapturesMask = UInt128.One << 2,
        };

        List<BitMove> expectedMoves = [quietMove, captureMove];
        List<BitMove> result = [.. moves[..moveCount]];

        result.Should().BeEquivalentTo(expectedMoves);
    }

    [Fact]
    public void CreateMoveFromQuiets_creates_moves_for_each_quiet_square()
    {
        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        BitPiece piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White };
        UInt128 quiets = (UInt128.One << 2) | (UInt128.One << 5);

        BitboardHelpers.CreateMoveFromQuiets(from, piece, quiets, moves, ref moveCount);

        BitMove[] expectedMoves =
        [
            new BitMove
            {
                From = 0,
                To = 2,
                Piece = piece,
            },
            new BitMove
            {
                From = 0,
                To = 5,
                Piece = piece,
            },
        ];

        List<BitMove> result = [.. moves[..moveCount]];
        result.Should().BeEquivalentTo(expectedMoves);
    }

    [Fact]
    public void CreateMoveFromCaptures_creates_moves_for_each_capture()
    {
        byte capturedPiece1Idx = 1;
        byte capturedPiece2Idx = 2;

        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        BitPiece piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White };
        UInt128 captures = (UInt128.One << capturedPiece1Idx) | (UInt128.One << capturedPiece2Idx);

        BitboardHelpers.CreateMoveFromCaptures(from, piece, captures, moves, ref moveCount);

        BitMove expectedMove1 = new()
        {
            From = from,
            To = capturedPiece1Idx,
            Piece = piece,
            CapturesMask = UInt128.One << capturedPiece1Idx,
        };

        BitMove expectedMove2 = new()
        {
            From = from,
            To = capturedPiece2Idx,
            Piece = piece,
            CapturesMask = UInt128.One << capturedPiece2Idx,
        };

        List<BitMove> expectedMoves = [expectedMove1, expectedMove2];
        List<BitMove> result = [.. moves[..moveCount]];

        result.Should().BeEquivalentTo(expectedMoves);
    }
}
