using AnarchyChess.Ai.Constants;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

struct CastleInfo
{
    public byte KingStart;
    public byte RookStart;
    public byte KingDest;
    public byte RookDest;
    public UInt128 BetweenMask;
    public SpecialMoveType MoveType;
}

public sealed class BitKingDefinition : IBitPieceDefinition
{
    // all castling masks don't include king and rook destinations, because we can capture our own bishop, so it's checked seperately
    private static readonly UInt128 WhiteKingSideBetweenMask =
        UInt128.One << new AlgebraicPoint("i1").AsIdx();
    private static readonly UInt128 BlackKingSideBetweenMask =
        UInt128.One << new AlgebraicPoint("i10").AsIdx();

    private static readonly UInt128 WhiteQueenSideBetweenMask =
        (UInt128.One << new AlgebraicPoint("b1").AsIdx())
        | (UInt128.One << new AlgebraicPoint("c1").AsIdx());
    private static readonly UInt128 BlackQueenSideBetweenMask =
        (UInt128.One << new AlgebraicPoint("b10").AsIdx())
        | (UInt128.One << new AlgebraicPoint("c10").AsIdx());

    private static readonly UInt128 WhiteVerticalBetweenMask =
        (UInt128.One << new AlgebraicPoint("f4").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f5").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f6").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f7").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f8").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f9").AsIdx());
    private static readonly UInt128 BlackVerticalBetweenMask =
        (UInt128.One << new AlgebraicPoint("f7").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f6").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f5").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f4").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f3").AsIdx())
        | (UInt128.One << new AlgebraicPoint("f2").AsIdx());

    private static readonly CastleInfo[] WhiteCastles =
    [
        new CastleInfo
        {
            KingStart = new AlgebraicPoint("f1").AsIdx(),
            RookStart = new AlgebraicPoint("j1").AsIdx(),
            KingDest = new AlgebraicPoint("h1").AsIdx(),
            RookDest = new AlgebraicPoint("g1").AsIdx(),
            BetweenMask = WhiteKingSideBetweenMask,
            MoveType = SpecialMoveType.KingsideCastle,
        },
        new CastleInfo
        {
            KingStart = new AlgebraicPoint("f1").AsIdx(),
            RookStart = new AlgebraicPoint("a1").AsIdx(),
            KingDest = new AlgebraicPoint("d1").AsIdx(),
            RookDest = new AlgebraicPoint("e1").AsIdx(),
            BetweenMask = WhiteQueenSideBetweenMask,
            MoveType = SpecialMoveType.QueensideCastle,
        },
        new CastleInfo
        {
            KingStart = new AlgebraicPoint("f1").AsIdx(),
            RookStart = new AlgebraicPoint("f10").AsIdx(),
            KingDest = new AlgebraicPoint("f3").AsIdx(),
            RookDest = new AlgebraicPoint("f2").AsIdx(),
            BetweenMask = WhiteVerticalBetweenMask,
            MoveType = SpecialMoveType.VerticalCastle,
        },
    ];

    private static readonly CastleInfo[] BlackCastles =
    [
        new CastleInfo
        {
            KingStart = new AlgebraicPoint("f10").AsIdx(),
            RookStart = new AlgebraicPoint("j10").AsIdx(),
            KingDest = new AlgebraicPoint("h10").AsIdx(),
            RookDest = new AlgebraicPoint("g10").AsIdx(),
            BetweenMask = BlackKingSideBetweenMask,
            MoveType = SpecialMoveType.KingsideCastle,
        },
        new CastleInfo
        {
            KingStart = new AlgebraicPoint("f10").AsIdx(),
            RookStart = new AlgebraicPoint("a10").AsIdx(),
            KingDest = new AlgebraicPoint("d10").AsIdx(),
            RookDest = new AlgebraicPoint("e10").AsIdx(),
            BetweenMask = BlackQueenSideBetweenMask,
            MoveType = SpecialMoveType.QueensideCastle,
        },
        new CastleInfo
        {
            KingStart = new AlgebraicPoint("f10").AsIdx(),
            RookStart = new AlgebraicPoint("f1").AsIdx(),
            KingDest = new AlgebraicPoint("f8").AsIdx(),
            RookDest = new AlgebraicPoint("f9").AsIdx(),
            BetweenMask = BlackVerticalBetweenMask,
            MoveType = SpecialMoveType.VerticalCastle,
        },
    ];

    public void GenerateMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 ownPieces = board.BitboardForFriendOf(color);
        UInt128 enemyPieces = board.BitboardForEnemyOf(color);

        UInt128 kingBit = UInt128.One << position;
        UInt128 attacks = 0;

        attacks |= (kingBit & ~BitboardConstants.RightEdgeMask) << 1; // right
        attacks |= (kingBit & ~BitboardConstants.LeftEdgeMask) >> 1; // left
        attacks |= (kingBit & ~BitboardConstants.TopEdgeMask) << 10; // up
        attacks |= (kingBit & ~BitboardConstants.BottomEdgeMask) >> 10; // down
        attacks |=
            (kingBit & ~(BitboardConstants.TopEdgeMask | BitboardConstants.RightEdgeMask)) << 11; // up right
        attacks |=
            (kingBit & ~(BitboardConstants.TopEdgeMask | BitboardConstants.LeftEdgeMask)) << 9; // up left
        attacks |=
            (kingBit & ~(BitboardConstants.BottomEdgeMask | BitboardConstants.RightEdgeMask)) >> 9; // bottom right
        attacks |=
            (kingBit & ~(BitboardConstants.BottomEdgeMask | BitboardConstants.LeftEdgeMask)) >> 11; // bottom left

        attacks &= ~ownPieces;

        while (attacks != 0)
        {
            int toSquare = BitboardHelpers.BitScanForward(ref attacks);
            bool isCapture = (enemyPieces & (UInt128.One << toSquare)) != 0;

            moves[moveCount++] = new BitMove()
            {
                From = position,
                To = (byte)toSquare,
                Piece = pieceType,
                Captures = isCapture ? (UInt128.One << toSquare) : 0,
            };
        }

        GenerateCastleMovesForColor(board, pieceType, color, position, moves, ref moveCount);
    }

    private static void GenerateCastleMovesForColor(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        CastleInfo[] castles = color is BitPieceColor.White ? WhiteCastles : BlackCastles;
        foreach (var castle in castles)
        {
            GenerateCastleMoves(board, pieceType, color, position, castle, moves, ref moveCount);
        }
    }

    private static void GenerateCastleMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
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
            (board.BitboardFor(pieceType, color) & kingStartMask) == 0
            || (board.BitboardFor(PieceType.Rook, color) & rookStartMask) == 0
        )
        {
            return;
        }

        if (board.HasPieceMoved(kingStartMask) || board.HasPieceMoved(rookStartMask))
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
        UInt128 bishopBitboard = board.BitboardFor(PieceType.Bishop, color);
        if ((bishopBitboard & kingDestMask) != 0)
        {
            captureMask = UInt128.One << castleInfo.KingDest;
        }
        else if ((bishopBitboard & rookDestMask) != 0)
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
            Piece = pieceType,
            Captures = captureMask,
            SpecialMoveType = castleInfo.MoveType,
        };
    }
}
