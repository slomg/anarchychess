using AnarchyChess.Ai.Magic.PiecesMagic;
using AnarchyChess.EngineShared;
using MessagePack;

namespace AnarchyChess.Ai.Magic;

public static class MagicGenerator
{
    public static void Generate(IPieceMagic magicPiece)
    {
        Console.WriteLine($"Started genearting magic table for {magicPiece.Name}");

        MagicPieceTable table = new()
        {
            Masks = new UInt128[Constants.SquareCount],
            MagicNumbers = new UInt128[Constants.SquareCount],
            Shifts = new int[Constants.SquareCount],
            AttackTable = new UInt128[Constants.SquareCount][],
        };

        for (int y = 0; y < Constants.BoardSize; y++)
        {
            for (int x = 0; x < Constants.BoardSize; x++)
            {
                int squareIdx = y * Constants.BoardSize + x;
                AlgebraicPoint point = new(X: x, Y: y);

                UInt128 mask = magicPiece.GenerateMask(point);
                table.Masks[squareIdx] = mask;
                var blockers = GenerateBlockerSubsets(mask);
                int shift = 128 - CountBits(mask);
                table.Shifts[squareIdx] = shift;

                int tableSize = 1 << CountBits(mask);
                table.AttackTable[squareIdx] = new UInt128[tableSize];

                UInt128 magic = FindMagic(blockers, mask, shift);
                table.MagicNumbers[squareIdx] = magic;

                for (int subsetIndex = 0; subsetIndex < blockers.Length; subsetIndex++)
                {
                    var blocker = blockers[subsetIndex];
                    var attackSet = magicPiece.ComputeAttacks(point, blocker);
                    int attackTableIndex = (int)(((blocker & mask) * magic) >> shift);
                    table.AttackTable[squareIdx][attackTableIndex] = attackSet;
                }

                Console.WriteLine(
                    $"Square {squareIdx}: mask={mask}, shift={shift}, magic={magic} ({squareIdx + 1}/{Constants.SquareCount})"
                );
            }
        }

        string folderPath = "MagicTables";
        string filePath = Path.Combine(folderPath, $"{magicPiece.Name}Magic.msgpack");
        Directory.CreateDirectory(folderPath);

        byte[] tableBytes = MessagePackSerializer.Serialize(table);
        File.WriteAllBytes(filePath, tableBytes);
        Console.WriteLine(
            $"Finished generating magic table for {magicPiece.Name}, saved to {filePath}"
        );
    }

    private static UInt128[] GenerateBlockerSubsets(UInt128 mask)
    {
        int bits = CountBits(mask);
        ulong subsetCount = 1UL << bits;

        UInt128[] subsets = new UInt128[subsetCount];
        int[] bitIndices = new int[bits];

        int idx = 0;
        for (int i = 0; i < Constants.SquareCount; i++)
        {
            if ((mask & (UInt128.One << i)) != 0)
            {
                bitIndices[idx++] = i;
            }
        }

        for (ulong subset = 0; subset < subsetCount; subset++)
        {
            UInt128 bitboard = 0;
            for (int bit = 0; bit < bits; bit++)
            {
                if ((subset & (1UL << bit)) != 0)
                {
                    bitboard |= UInt128.One << bitIndices[bit];
                }
            }
            subsets[subset] = bitboard;
        }

        return subsets;
    }

    private static int CountBits(UInt128 mask)
    {
        int count = 0;
        while (mask != 0)
        {
            mask &= mask - 1;
            count++;
        }
        return count;
    }

    private static UInt128 FindMagic(UInt128[] blockers, UInt128 mask, int shift)
    {
        Random random = new();
        while (true)
        {
            UInt128 candidate = NextMagic(random);
            HashSet<UInt128> usedIndxs = [];
            bool fail = false;
            foreach (UInt128 blocker in blockers)
            {
                UInt128 index = ((blocker & mask) * candidate) >> shift;
                if (!usedIndxs.Add(index))
                {
                    fail = true;
                    break;
                }
            }
            if (!fail)
            {
                return candidate;
            }
        }
    }

    private static UInt128 NextMagic(Random random)
    {
        return NextUInt128(random) & NextUInt128(random) & NextUInt128(random);
    }

    private static UInt128 NextUInt128(Random random)
    {
        ulong high = (ulong)random.NextInt64(long.MinValue, long.MaxValue);
        ulong low = (ulong)random.NextInt64(long.MinValue, long.MaxValue);

        return new UInt128(high, low);
    }
}
