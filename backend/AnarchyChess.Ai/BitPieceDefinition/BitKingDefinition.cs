using AnarchyChess.Ai.Helpers;

namespace AnarchyChess.Ai.BitPieceDefinition;

struct CastleInfo
{
    public byte KingStart;
    public byte RookStart;
    public byte KingDest;
    public byte RookDest;
    public UInt128 BetweenMask;
    public BitMoveFlag Flag;
}

public sealed class BitKingDefinition : IBitPieceDefinition
{
    public BitPiece PieceType => BitPiece.King;

    // all castling masks don't include king and rook destinations, because we can capture our own bishop, so it's checked seperately
    private static readonly UInt128 WhiteKingSideBetweenMask = UInt128.One << 8; // i1
    private static readonly UInt128 BlackKingSideBetweenMask = UInt128.One << 98; // i10

    private static readonly UInt128 WhiteQueenSideBetweenMask =
        (UInt128.One << 1) | (UInt128.One << 2); // b1, c1;
    private static readonly UInt128 BlackQueenSideBetweenMask =
        (UInt128.One << 91) | (UInt128.One << 92); // b10, c10;

    private static readonly UInt128 WhiteVerticalBetweenMask =
        (UInt128.One << 35)
        | (UInt128.One << 45)
        | (UInt128.One << 55)
        | (UInt128.One << 65)
        | (UInt128.One << 75)
        | (UInt128.One << 85); // f4 - f9
    private static readonly UInt128 BlackVerticalBetweenMask =
        (UInt128.One << 65)
        | (UInt128.One << 55)
        | (UInt128.One << 45)
        | (UInt128.One << 35)
        | (UInt128.One << 25)
        | (UInt128.One << 15); // f7 - f2

    private static readonly CastleInfo[] WhiteCastles =
    [
        new CastleInfo
        {
            KingStart = 5, // f1
            RookStart = 9, // j1
            KingDest = 7, // h1
            RookDest = 6, // g1
            BetweenMask = WhiteKingSideBetweenMask,
            Flag = BitMoveFlag.KingSideCastling,
        },
        new CastleInfo
        {
            KingStart = 5, // f1
            RookStart = 0, // a1
            KingDest = 3, // d1
            RookDest = 4, // e1
            BetweenMask = WhiteQueenSideBetweenMask,
            Flag = BitMoveFlag.QueenSideCastling,
        },
        new CastleInfo
        {
            KingStart = 5, // f1
            RookStart = 95, // f10
            KingDest = 25, // f3
            RookDest = 15, // f2
            BetweenMask = WhiteVerticalBetweenMask,
            Flag = BitMoveFlag.VerticalCastling,
        },
    ];

    private static readonly CastleInfo[] BlackCastles =
    [
        new CastleInfo
        {
            KingStart = 95, // f10
            RookStart = 99, // j10
            KingDest = 97, // h10
            RookDest = 96, // g10
            BetweenMask = BlackKingSideBetweenMask,
            Flag = BitMoveFlag.KingSideCastling,
        },
        new CastleInfo
        {
            KingStart = 95, // f10
            RookStart = 90, // a10
            KingDest = 93, // d10
            RookDest = 94, // e10
            BetweenMask = BlackQueenSideBetweenMask,
            Flag = BitMoveFlag.QueenSideCastling,
        },
        new CastleInfo
        {
            KingStart = 95, // f10
            RookStart = 5, // f1
            KingDest = 75, // f8
            RookDest = 85, // f9
            BetweenMask = BlackVerticalBetweenMask,
            Flag = BitMoveFlag.VerticalCastling,
        },
    ];

    public void GenerateMoves(
        BitBoard board,
        BitColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 ownPieces = board.BitboardForFriendOf(color);
        UInt128 enemyPieces = board.BitboardForEnemyOf(color);

        UInt128 targets = 0;

        targets |= (position & ~BitboardConstants.RightEdgeMask) << 1; // right
        targets |= (position & ~BitboardConstants.LeftEdgeMask) >> 1; // left
        targets |= (position & ~BitboardConstants.TopEdgeMask) << 10; // up
        targets |= (position & ~BitboardConstants.BottomEdgeMask) >> 10; // down
        targets |=
            (position & ~(BitboardConstants.TopEdgeMask | BitboardConstants.RightEdgeMask)) << 11; // up right
        targets |=
            (position & ~(BitboardConstants.TopEdgeMask | BitboardConstants.LeftEdgeMask)) << 9; // up left
        targets |=
            (position & ~(BitboardConstants.BottomEdgeMask | BitboardConstants.RightEdgeMask)) >> 9; // bottom right
        targets |=
            (position & ~(BitboardConstants.BottomEdgeMask | BitboardConstants.LeftEdgeMask)) >> 11; // bottom left

        targets &= ~ownPieces;

        while (targets != 0)
        {
            int toSquare = BitboardHelpers.BitScanForward(ref targets);
            bool isCapture = (enemyPieces & (UInt128.One << toSquare)) != 0;

            moves[moveCount++] = new BitMove()
            {
                From = position,
                To = (byte)toSquare,
                Piece = BitPiece.King,
                Captures = isCapture ? (UInt128.One << toSquare) : 0,
            };
        }

        GenerateCastleMovesForColor(board, color, position, moves, ref moveCount);
    }

    private static void GenerateCastleMovesForColor(
        BitBoard board,
        BitColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        CastleInfo[] castles = color is BitColor.White ? WhiteCastles : BlackCastles;
        foreach (var castle in castles)
        {
            GenerateCastleMoves(board, color, position, castle, moves, ref moveCount);
        }
    }

    private static void GenerateCastleMoves(
        BitBoard board,
        BitColor color,
        byte position,
        CastleInfo castleInfo,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        if (position != castleInfo.KingStart)
        {
            return;
        }

        UInt128 kingStartMask = UInt128.One << castleInfo.KingStart;
        UInt128 rookStartMask = UInt128.One << castleInfo.RookStart;

        if (
            (board.BitboardFor(BitPiece.King, color) & kingStartMask) == 0
            || (board.BitboardFor(BitPiece.Rook, color) & rookStartMask) == 0
        )
        {
            return;
        }

        if ((board.HasMoved & kingStartMask) != 0 || (board.HasMoved & rookStartMask) != 0)
        {
            return;
        }

        if ((board.Occupancy & castleInfo.BetweenMask) != 0)
        {
            return;
        }

        UInt128 kingDestMask = UInt128.One << castleInfo.KingDest;
        UInt128 rookDestMask = UInt128.One << castleInfo.RookDest;

        UInt128 captureMask = 0;
        if ((board.BitboardFor(BitPiece.Bishop, color) & kingDestMask) != 0)
        {
            captureMask = UInt128.One << castleInfo.KingDest;
        }
        else if ((board.BitboardFor(BitPiece.Bishop, color) & rookDestMask) != 0)
        {
            captureMask = UInt128.One << castleInfo.RookDest;
        }
        else if ((board.Occupancy & kingDestMask) != 0 || (board.Occupancy & rookDestMask) != 0)
        {
            return;
        }

        moves[moveCount++] = new BitMove()
        {
            From = position,
            To = castleInfo.KingDest,
            Captures = captureMask,
            Flags = castleInfo.Flag,
        };
    }
}
