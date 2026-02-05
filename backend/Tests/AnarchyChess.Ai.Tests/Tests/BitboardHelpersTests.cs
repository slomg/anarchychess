using AnarchyChess.Ai.Helpers;
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
}
