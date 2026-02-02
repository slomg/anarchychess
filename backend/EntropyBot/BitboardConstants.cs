namespace AnarchyBot;

public static class BitboardConstants
{
    public static readonly UInt128 LeftEdgeMask = MakeFileMask(0);
    public static readonly UInt128 RightEdgeMask = MakeFileMask(9);
    public static readonly UInt128 BottomEdgeMask = MakeRankMask(0);
    public static readonly UInt128 TopEdgeMask = MakeRankMask(9);

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
}
