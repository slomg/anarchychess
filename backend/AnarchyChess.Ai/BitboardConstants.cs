using AnarchyChess.EngineShared;

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

    // all castling masks don't include king and rook destinations, because we can capture our own bishop, so it's checked seperately
    public static readonly UInt128 WhiteKingSideBetweenMask =
        UInt128.One << new AlgebraicPoint("i1").AsIdx();
    public static readonly UInt128 BlackKingSideBetweenMask =
        UInt128.One << new AlgebraicPoint("i10").AsIdx();

    public static readonly UInt128 WhiteQueenSideBetweenMask =
        (UInt128.One << new AlgebraicPoint("b1").AsIdx())
        | (UInt128.One << new AlgebraicPoint("c1").AsIdx());
    public static readonly UInt128 BlackQueenSideBetweenMask =
        (UInt128.One << new AlgebraicPoint("b10").AsIdx())
        | (UInt128.One << new AlgebraicPoint("c10").AsIdx());

    public static readonly UInt128 WhiteVerticalBetweenMask =
        (UInt128.One << new AlgebraicPoint("f4").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f5").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f6").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f7").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f8").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f9").AsIdx());
    public static readonly UInt128 BlackVerticalBetweenMask =
        (UInt128.One << new AlgebraicPoint("f7").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f6").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f5").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f4").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f3").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f2").AsIdx());

    public static readonly CastleInfo WhiteKingsideCastle = new()
    {
        KingStart = new AlgebraicPoint("f1").AsIdx(),
        RookStart = new AlgebraicPoint("j1").AsIdx(),
        KingDest = new AlgebraicPoint("h1").AsIdx(),
        RookDest = new AlgebraicPoint("g1").AsIdx(),
        BetweenMask = WhiteKingSideBetweenMask,
        MoveType = SpecialMoveType.KingsideCastle,
    };
    public static readonly CastleInfo WhiteQueensideCastle = new()
    {
        KingStart = new AlgebraicPoint("f1").AsIdx(),
        RookStart = new AlgebraicPoint("a1").AsIdx(),
        KingDest = new AlgebraicPoint("d1").AsIdx(),
        RookDest = new AlgebraicPoint("e1").AsIdx(),
        BetweenMask = WhiteQueenSideBetweenMask,
        MoveType = SpecialMoveType.QueensideCastle,
    };
    public static readonly CastleInfo WhiteVerticalCastle = new()
    {
        KingStart = new AlgebraicPoint("f1").AsIdx(),
        RookStart = new AlgebraicPoint("f10").AsIdx(),
        KingDest = new AlgebraicPoint("f3").AsIdx(),
        RookDest = new AlgebraicPoint("f2").AsIdx(),
        BetweenMask = WhiteVerticalBetweenMask,
        MoveType = SpecialMoveType.VerticalCastle,
    };

    public static readonly CastleInfo BlackKingsideCastle = new()
    {
        KingStart = new AlgebraicPoint("f10").AsIdx(),
        RookStart = new AlgebraicPoint("j10").AsIdx(),
        KingDest = new AlgebraicPoint("h10").AsIdx(),
        RookDest = new AlgebraicPoint("g10").AsIdx(),
        BetweenMask = BlackKingSideBetweenMask,
        MoveType = SpecialMoveType.KingsideCastle,
    };
    public static readonly CastleInfo BlackQueensideCastle = new()
    {
        KingStart = new AlgebraicPoint("f10").AsIdx(),
        RookStart = new AlgebraicPoint("a10").AsIdx(),
        KingDest = new AlgebraicPoint("d10").AsIdx(),
        RookDest = new AlgebraicPoint("e10").AsIdx(),
        BetweenMask = BlackQueenSideBetweenMask,
        MoveType = SpecialMoveType.QueensideCastle,
    };
    public static readonly CastleInfo BlackVerticalCastle = new()
    {
        KingStart = new AlgebraicPoint("f10").AsIdx(),
        RookStart = new AlgebraicPoint("f1").AsIdx(),
        KingDest = new AlgebraicPoint("f8").AsIdx(),
        RookDest = new AlgebraicPoint("f9").AsIdx(),
        BetweenMask = BlackVerticalBetweenMask,
        MoveType = SpecialMoveType.VerticalCastle,
    };

    // BitPieceColor, CastleType
    public static readonly CastleInfo[,] CastlesByColor = new CastleInfo[2, 3]
    {
        { WhiteKingsideCastle, WhiteQueensideCastle, WhiteVerticalCastle },
        { BlackKingsideCastle, BlackQueensideCastle, BlackVerticalCastle },
    };

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
