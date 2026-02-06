namespace AnarchyChess.Ai;

public static class BitboardConstants
{
    public static readonly UInt128 LeftEdgeMask = MakeFileMask(0);
    public static readonly UInt128 RightEdgeMask = MakeFileMask(9);
    public static readonly UInt128 BottomEdgeMask = MakeRankMask(0);
    public static readonly UInt128 TopEdgeMask = MakeRankMask(9);

    public static readonly UInt128 EdgeMasks =
        LeftEdgeMask | RightEdgeMask | BottomEdgeMask | TopEdgeMask;

    public static readonly UInt128[] HorseyMasks = MakeHorseyMasks();

    private static UInt128 MakeFileMask(int file)
    {
        UInt128 mask = 0;
        for (int rank = 0; rank < 10; rank++)
        {
            mask |= UInt128.One << (rank * 10 + file);
        }
        return mask;
    }

    private static UInt128 MakeRankMask(int rank)
    {
        UInt128 mask = 0;
        for (int file = 0; file < 10; file++)
        {
            mask |= UInt128.One << (rank * 10 + file);
        }
        return mask;
    }

    private static UInt128[] MakeHorseyMasks()
    {
        UInt128[] knightMasks = new UInt128[10 * 10];
        for (int rank = 0; rank < 10; rank++)
        {
            for (int file = 0; file < 10; file++)
            {
                int squareIdx = rank * 10 + file;
                knightMasks[squareIdx] = MakeHorseyMaskForSquare(rank, file);
            }
        }
        return knightMasks;
    }

    private static UInt128 MakeHorseyMaskForSquare(int rank, int file)
    {
        UInt128 mask = 0;

        int[] deltaRanks = [-2, -1, 1, 2, 2, 1, -1, -2];
        int[] deltaFiles = [1, 2, 2, 1, -1, -2, -2, -1];

        for (int i = 0; i < 8; i++)
        {
            int deltaRank = rank + deltaRanks[i];
            int deltaFile = file + deltaFiles[i];
            if (deltaRank >= 0 && deltaRank < 10 && deltaFile >= 0 && deltaFile < 10)
            {
                mask |= UInt128.One << (deltaRank * 10 + deltaFile);
            }
        }

        return mask;
    }
}
