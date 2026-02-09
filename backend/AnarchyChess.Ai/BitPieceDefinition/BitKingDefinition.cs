using AnarchyChess.Ai.Helpers;
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
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 friendlyPieces = board.BitboardForFriendOf(piece.Color);

        UInt128 attacks = BitboardHelpers.MaskAdjacent(position);
        attacks &= ~friendlyPieces;

        BitboardHelpers.CreateMoveFromAttacks(
            position,
            piece,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );
        GenerateCastleMovesForColor(board, piece, position, moves, ref moveCount);
    }

    private static void GenerateCastleMovesForColor(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        CastleInfo[] castles = piece.Color is BitPieceColor.White ? WhiteCastles : BlackCastles;
        foreach (var castle in castles)
        {
            GenerateCastleMoves(board, piece, position, castle, moves, ref moveCount);
        }
    }

    private static void GenerateCastleMoves(
        BitBoard board,
        BitPiece piece,
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
            (board.BitboardFor(piece.Type, piece.Color) & kingStartMask) == 0
            || (board.BitboardFor(PieceType.Rook, piece.Color) & rookStartMask) == 0
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

        UInt128 bishopCaptureMask = 0;
        UInt128 bishopBitboard = board.BitboardFor(PieceType.Bishop, piece.Color);
        if ((bishopBitboard & kingDestMask) != 0)
        {
            bishopCaptureMask = UInt128.One << castleInfo.KingDest;
        }
        else if ((bishopBitboard & rookDestMask) != 0)
        {
            bishopCaptureMask = UInt128.One << castleInfo.RookDest;
        }
        else if ((board.Occupancy & kingDestMask) != 0 || (board.Occupancy & rookDestMask) != 0)
        {
            return;
        }

        BitMove move = new()
        {
            From = position,
            To = castleInfo.KingDest,
            Piece = piece,
            CapturesMask = bishopCaptureMask,
            SpecialMoveType = castleInfo.MoveType,
        };
        moves[moveCount++] = move;
    }
}
