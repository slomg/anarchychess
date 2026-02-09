namespace AnarchyChess.Ai;

public static class BitboardConstants
{
    public static readonly UInt128 LeftEdgeMask = MakeFileMask(0);
    public static readonly UInt128 RightEdgeMask = MakeFileMask(9);
    public static readonly UInt128 BottomEdgeMask = MakeRankMask(0);
    public static readonly UInt128 TopEdgeMask = MakeRankMask(9);

    public static readonly UInt128 TopRightEdgeMask = TopEdgeMask | RightEdgeMask;
    public static readonly UInt128 TopLeftEdgeMask = TopEdgeMask | LeftEdgeMask;
    public static readonly UInt128 BottomRightEdgeMask = BottomEdgeMask | RightEdgeMask;
    public static readonly UInt128 BottomLeftEdgeMask = BottomEdgeMask | LeftEdgeMask;

    public static readonly UInt128 EdgeMasks =
        LeftEdgeMask | RightEdgeMask | BottomEdgeMask | TopEdgeMask;

    public static readonly UInt128 LeftEdgeExcludeMask = ~LeftEdgeMask;
    public static readonly UInt128 RightEdgeExcludeMask = ~RightEdgeMask;
    public static readonly UInt128 BottomEdgeExcludeMask = ~BottomEdgeMask;
    public static readonly UInt128 TopEdgeExcludeMask = ~TopEdgeMask;

    public static readonly UInt128 TopRightEdgeExcludeMask = ~TopRightEdgeMask;
    public static readonly UInt128 TopLeftEdgeExcludeMask = ~TopLeftEdgeMask;
    public static readonly UInt128 BottomRightEdgeExcludeMask = ~BottomRightEdgeMask;
    public static readonly UInt128 BottomLeftEdgeExcludeMask = ~BottomLeftEdgeMask;

    public static readonly UInt128[] HorseyMasks = MakeBoardMasksByDeltas(
        deltaRanks: [-2, -1, 1, 2, 2, 1, -1, -2],
        deltaFiles: [1, 2, 2, 1, -1, -2, -2, -1]
    );

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

    private static UInt128[] MakeBoardMasksByDeltas(int[] deltaRanks, int[] deltaFiles)
    {
        UInt128[] masks = new UInt128[10 * 10];
        for (int rank = 0; rank < 10; rank++)
        {
            for (int file = 0; file < 10; file++)
            {
                int squareIdx = rank * 10 + file;
                masks[squareIdx] = MakeMaskForSquareByDeltas(rank, file, deltaRanks, deltaFiles);
            }
        }
        return masks;
    }

    private static UInt128 MakeMaskForSquareByDeltas(
        int rank,
        int file,
        int[] deltaRanks,
        int[] deltaFiles
    )
    {
        UInt128 mask = 0;

        for (int i = 0; i < Math.Max(deltaRanks.Length, deltaFiles.Length); i++)
        {
            int deltaRank = i < deltaRanks.Length ? rank + deltaRanks[i] : 0;
            int deltaFile = i < deltaFiles.Length ? file + deltaFiles[i] : 0;
            if (deltaRank >= 0 && deltaRank < 10 && deltaFile >= 0 && deltaFile < 10)
            {
                mask |= UInt128.One << (deltaRank * 10 + deltaFile);
            }
        }

        return mask;
    }
}
