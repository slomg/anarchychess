using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public static class PieceMasks
{
    public static readonly UInt128[] HorseyMasks = MakeBoardMasks(
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

    public static readonly UInt128[] AdjacentMasks = MakeBoardMasks(
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

    public static readonly UInt128[] SingleCheckerJumpMasks = MakeBoardMasks(
        [
            new Offset(X: -2, Y: 2),
            new Offset(X: 2, Y: 2),
            new Offset(X: -2, Y: -2),
            new Offset(X: 2, Y: -2),
        ]
    );

    public static readonly UInt128[] WhiteThrowForwardMasks = MakeBoardMasks(
        [
            // 1
            new Offset(X: -1, Y: 1),
            new Offset(X: 0, Y: 1),
            new Offset(X: 1, Y: 1),
            // 2
            new Offset(X: -1, Y: 2),
            new Offset(X: 0, Y: 2),
            new Offset(X: 1, Y: 2),
            // 3
            new Offset(X: -1, Y: 3),
            new Offset(X: 0, Y: 3),
            new Offset(X: 1, Y: 3),
            // 4
            new Offset(X: -1, Y: 4),
            new Offset(X: 0, Y: 4),
            new Offset(X: 1, Y: 4),
            // 5
            new Offset(X: -1, Y: 5),
            new Offset(X: 0, Y: 5),
            new Offset(X: 1, Y: 5),
            // 6
            new Offset(X: -1, Y: 6),
            new Offset(X: 0, Y: 6),
            new Offset(X: 1, Y: 6),
            // 7
            new Offset(X: -1, Y: 7),
            new Offset(X: 0, Y: 7),
            new Offset(X: 1, Y: 7),
            // 8
            new Offset(X: -1, Y: 8),
            new Offset(X: 0, Y: 8),
            new Offset(X: 1, Y: 8),
        ],
        predicate: (point, offset) => point.Y <= 8
    );

    public static readonly UInt128[] BlackThrowForwardMasks = MakeBoardMasks(
        [
            // 1
            new Offset(X: -1, Y: -1),
            new Offset(X: 0, Y: -1),
            new Offset(X: 1, Y: -1),
            // 2
            new Offset(X: -1, Y: -2),
            new Offset(X: 0, Y: -2),
            new Offset(X: 1, Y: -2),
            // 3
            new Offset(X: -1, Y: -3),
            new Offset(X: 0, Y: -3),
            new Offset(X: 1, Y: -3),
            // 4
            new Offset(X: -1, Y: -4),
            new Offset(X: 0, Y: -4),
            new Offset(X: 1, Y: -4),
            // 5
            new Offset(X: -1, Y: -5),
            new Offset(X: 0, Y: -5),
            new Offset(X: 1, Y: -5),
            // 6
            new Offset(X: -1, Y: -6),
            new Offset(X: 0, Y: -6),
            new Offset(X: 1, Y: -6),
            // 7
            new Offset(X: -1, Y: -7),
            new Offset(X: 0, Y: -7),
            new Offset(X: 1, Y: -7),
            // 8
            new Offset(X: -1, Y: -8),
            new Offset(X: 0, Y: -8),
            new Offset(X: 1, Y: -8),
        ],
        predicate: (point, offset) => point.Y >= 1
    );

    public static readonly UInt128[] WhiteThrowLeftMasks = MakeBoardMasks(
        [
            // 1
            new Offset(X: -1, Y: 0),
            new Offset(X: -1, Y: 1),
            new Offset(X: -1, Y: 2),
            // 2
            new Offset(X: -2, Y: 1),
            new Offset(X: -2, Y: 2),
            new Offset(X: -2, Y: 3),
            // 3
            new Offset(X: -3, Y: 2),
            new Offset(X: -3, Y: 3),
            new Offset(X: -3, Y: 4),
            // 4
            new Offset(X: -4, Y: 3),
            new Offset(X: -4, Y: 4),
            new Offset(X: -4, Y: 5),
            // 5
            new Offset(X: -5, Y: 4),
            new Offset(X: -5, Y: 5),
            new Offset(X: -5, Y: 6),
            // 6
            new Offset(X: -6, Y: 5),
            new Offset(X: -6, Y: 6),
            new Offset(X: -6, Y: 7),
            // 7
            new Offset(X: -7, Y: 6),
            new Offset(X: -7, Y: 7),
            new Offset(X: -7, Y: 8),
            // 8
            new Offset(X: -8, Y: 7),
            new Offset(X: -8, Y: 8),
            new Offset(X: -8, Y: 9),
            // 9
            new Offset(X: -9, Y: 8),
            new Offset(X: -9, Y: 9),
        ],
        predicate: (point, offset) => point.Y <= 8
    );

    public static readonly UInt128[] BlackThrowLeftMasks = MakeBoardMasks(
        [
            // 1
            new Offset(X: -1, Y: 0),
            new Offset(X: -1, Y: -1),
            new Offset(X: -1, Y: -2),
            // 2
            new Offset(X: -2, Y: -1),
            new Offset(X: -2, Y: -2),
            new Offset(X: -2, Y: -3),
            // 3
            new Offset(X: -3, Y: -2),
            new Offset(X: -3, Y: -3),
            new Offset(X: -3, Y: -4),
            // 4
            new Offset(X: -4, Y: -3),
            new Offset(X: -4, Y: -4),
            new Offset(X: -4, Y: -5),
            // 5
            new Offset(X: -5, Y: -4),
            new Offset(X: -5, Y: -5),
            new Offset(X: -5, Y: -6),
            // 6
            new Offset(X: -6, Y: -5),
            new Offset(X: -6, Y: -6),
            new Offset(X: -6, Y: -7),
            // 7
            new Offset(X: -7, Y: -6),
            new Offset(X: -7, Y: -7),
            new Offset(X: -7, Y: -8),
            // 8
            new Offset(X: -8, Y: -7),
            new Offset(X: -8, Y: -8),
            new Offset(X: -8, Y: -9),
            // 9
            new Offset(X: -9, Y: -8),
            new Offset(X: -9, Y: -9),
        ],
        predicate: (point, offset) => point.Y >= 1
    );

    public static readonly UInt128[] WhiteThrowRightMasks = MakeBoardMasks(
        [
            // 1
            new Offset(X: 1, Y: 0),
            new Offset(X: 1, Y: 1),
            new Offset(X: 1, Y: 2),
            // 2
            new Offset(X: 2, Y: 1),
            new Offset(X: 2, Y: 2),
            new Offset(X: 2, Y: 3),
            // 3
            new Offset(X: 3, Y: 2),
            new Offset(X: 3, Y: 3),
            new Offset(X: 3, Y: 4),
            // 4
            new Offset(X: 4, Y: 3),
            new Offset(X: 4, Y: 4),
            new Offset(X: 4, Y: 5),
            // 5
            new Offset(X: 5, Y: 4),
            new Offset(X: 5, Y: 5),
            new Offset(X: 5, Y: 6),
            // 6
            new Offset(X: 6, Y: 5),
            new Offset(X: 6, Y: 6),
            new Offset(X: 6, Y: 7),
            // 7
            new Offset(X: 7, Y: 6),
            new Offset(X: 7, Y: 7),
            new Offset(X: 7, Y: 8),
            // 8
            new Offset(X: 8, Y: 7),
            new Offset(X: 8, Y: 8),
            new Offset(X: 8, Y: 9),
            // 9
            new Offset(X: 9, Y: 8),
            new Offset(X: 9, Y: 9),
        ],
        predicate: (point, offset) => point.Y <= 8
    );

    public static readonly UInt128[] BlackThrowRightMasks = MakeBoardMasks(
        [
            // 1
            new Offset(X: 1, Y: 0),
            new Offset(X: 1, Y: -1),
            new Offset(X: 1, Y: -2),
            // 2
            new Offset(X: 2, Y: -1),
            new Offset(X: 2, Y: -2),
            new Offset(X: 2, Y: -3),
            // 3
            new Offset(X: 3, Y: -2),
            new Offset(X: 3, Y: -3),
            new Offset(X: 3, Y: -4),
            // 4
            new Offset(X: 4, Y: -3),
            new Offset(X: 4, Y: -4),
            new Offset(X: 4, Y: -5),
            // 5
            new Offset(X: 5, Y: -4),
            new Offset(X: 5, Y: -5),
            new Offset(X: 5, Y: -6),
            // 6
            new Offset(X: 6, Y: -5),
            new Offset(X: 6, Y: -6),
            new Offset(X: 6, Y: -7),
            // 7
            new Offset(X: 7, Y: -6),
            new Offset(X: 7, Y: -7),
            new Offset(X: 7, Y: -8),
            // 8
            new Offset(X: 8, Y: -7),
            new Offset(X: 8, Y: -8),
            new Offset(X: 8, Y: -9),
            // 9
            new Offset(X: 9, Y: -8),
            new Offset(X: 9, Y: -9),
        ],
        predicate: (point, offset) => point.Y >= 1
    );

    private static UInt128[] MakeBoardMasks(
        Offset[] offsets,
        Func<AlgebraicPoint, Offset, bool>? predicate = null
    )
    {
        UInt128[] masks = new UInt128[10 * 10];
        for (int rank = 0; rank < 10; rank++)
        {
            for (int file = 0; file < 10; file++)
            {
                int squareIdx = rank * 10 + file;
                masks[squareIdx] = MakeMaskForSquare(rank, file, offsets, predicate);
            }
        }
        return masks;
    }

    private static UInt128 MakeMaskForSquare(
        int rank,
        int file,
        Offset[] offsets,
        Func<AlgebraicPoint, Offset, bool>? predicate
    )
    {
        UInt128 mask = 0;

        foreach (Offset offset in offsets)
        {
            int deltaRank = rank + offset.Y;
            int deltaFile = file + offset.X;
            if (
                deltaRank >= 0
                && deltaRank < 10
                && deltaFile >= 0
                && deltaFile < 10
                && (predicate?.Invoke(new(X: deltaFile, Y: deltaRank), offset) ?? true)
            )
            {
                mask |= UInt128.One << (deltaRank * 10 + deltaFile);
            }
        }

        return mask;
    }
}
