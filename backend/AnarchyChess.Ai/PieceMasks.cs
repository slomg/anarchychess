using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public static class PieceMasks
{
    public static readonly UInt128[] HorseyMasks = MakeBoardMasksByDeltas(
        [
            new Offset(X: 1, Y: -2),
            new Offset(X: 2, Y: -1),
            new Offset(X: 2, Y: 1),
            new Offset(X: 1, Y: 2),
            new Offset(X: -1, Y: 2),
            new Offset(X: -2, Y: 1),
            new Offset(X: -2, Y: -1),
            new Offset(X: -1, Y: -2),
        ]
    );

    public static readonly UInt128[] AdjacentMasks = MakeBoardMasksByDeltas(
        [
            new Offset(X: -1, Y: -1),
            new Offset(X: 0, Y: -1),
            new Offset(X: 1, Y: -1),
            new Offset(X: -1, Y: 0),
            new Offset(X: 1, Y: 0),
            new Offset(X: -1, Y: 1),
            new Offset(X: 0, Y: 1),
            new Offset(X: 1, Y: 1),
        ]
    );

    public static readonly UInt128[] SingleCheckerJumpMasks = MakeBoardMasksByDeltas(
        [
            new Offset(X: -2, Y: 2),
            new Offset(X: 2, Y: 2),
            new Offset(X: -2, Y: -2),
            new Offset(X: 2, Y: -2),
        ]
    );

    private static UInt128[] MakeBoardMasksByDeltas(Offset[] offsets)
    {
        UInt128[] masks = new UInt128[10 * 10];
        for (int rank = 0; rank < 10; rank++)
        {
            for (int file = 0; file < 10; file++)
            {
                int squareIdx = rank * 10 + file;
                masks[squareIdx] = MakeMaskForSquareByDeltas(rank, file, offsets);
            }
        }
        return masks;
    }

    private static UInt128 MakeMaskForSquareByDeltas(int rank, int file, Offset[] offsets)
    {
        UInt128 mask = 0;

        foreach (Offset offset in offsets)
        {
            int deltaRank = rank + offset.Y;
            int deltaFile = file + offset.X;
            if (deltaRank >= 0 && deltaRank < 10 && deltaFile >= 0 && deltaFile < 10)
            {
                mask |= UInt128.One << (deltaRank * 10 + deltaFile);
            }
        }

        return mask;
    }
}
