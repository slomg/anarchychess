using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class MagicLibraryTests
{
    private static MagicPieceTable CreateTestTable()
    {
        MagicPieceTable table = new()
        {
            Masks = new UInt128[64],
            MagicNumbers = new UInt128[64],
            Shifts = new int[64],
            AttackTable = new UInt128[64][],
        };

        for (int i = 0; i < 64; i++)
        {
            table.Masks[i] = 0b1111; // only first 4 bits are relevant
            table.MagicNumbers[i] = 1;
            table.Shifts[i] = 0;
            table.AttackTable[i] = new UInt128[16]; // 4-bit index
            for (int j = 0; j < 16; j++)
            {
                table.AttackTable[i][j] = (UInt128)(j * 10);
            }
        }

        return table;
    }

    [Fact]
    public void GetAttacks_returns_correct_attack_from_mock_table()
    {
        var table = CreateTestTable();

        UInt128 occupancy = 0b1010;
        int square = 0;

        UInt128 result = MagicLibrary.GetAttacks(table, square, occupancy);

        // blockers = occupancy & mask = 0b1010 & 0b1111 = 0b1010 = 10
        // index = blockers * magic >> shift = 10 * 1 >> 0 = 10
        // attack = AttackTable[square][index] = 10 * 10 = 100
        result.Should().Be(100);
    }

    [Fact]
    public void GetAttacks_returns_zero_for_no_blockers()
    {
        var table = CreateTestTable();

        UInt128 occupancy = 0b0000;
        int square = 0;

        UInt128 result = MagicLibrary.GetAttacks(table, square, occupancy);

        // blockers = 0 -> index = 0 -> AttackTable[0][0] = 0
        result.Should().Be(0);
    }

    [Fact]
    public void GetAttacks_handles_max_index()
    {
        var table = CreateTestTable();

        UInt128 occupancy = 0b1111; // max for 4-bit mask
        int square = 0;

        UInt128 result = MagicLibrary.GetAttacks(table, square, occupancy);

        // index = 15 -> AttackTable[0][15] = 150
        result.Should().Be(150);
    }

    [Fact]
    public void Static_constructor_loads_tables()
    {
        MagicLibrary.RookTable.Should().NotBeNull();
        MagicLibrary.BishopTable.Should().NotBeNull();
        MagicLibrary.TwoStraightSquaresTable.Should().NotBeNull();
        MagicLibrary.WhiteLeftEnPassantTable.Should().NotBeNull();
        MagicLibrary.BlackLeftEnPassantTable.Should().NotBeNull();
    }
}
