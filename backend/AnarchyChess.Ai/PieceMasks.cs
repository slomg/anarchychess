namespace AnarchyChess.Ai;

public static class PieceMasks
{
    public static readonly UInt128[] HorseyMasks = MakeBoardMasksByDeltas(
        deltaRanks: [-2, -1, 1, 2, 2, 1, -1, -2],
        deltaFiles: [1, 2, 2, 1, -1, -2, -2, -1]
    );

    public static readonly UInt128[] AdjacentMasks = MakeBoardMasksByDeltas(
        deltaRanks: [-1, -1, -1, 0, 0, 1, 1, 1],
        deltaFiles: [-1, 0, 1, -1, 1, -1, 0, 1]
    );

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
