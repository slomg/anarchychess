using AnarchyChess.Ai.Constants;
using AnarchyChess.Ai.Extensions;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public struct BitBoard
{
    public UInt128[] Bitboards;

    public UInt128 WhitePieces;
    public UInt128 BlackPieces;
    public UInt128 NeutralPieces;
    public UInt128 Occupancy;

    public UInt128 HasMoved;

    public BitBoard(UInt128[]? bitboards = null, UInt128? hasMoved = null)
    {
        Bitboards = bitboards ?? new UInt128[Enum.GetValues<BitPieceType>().Length];
        HasMoved = hasMoved ?? 0;

        for (int i = 0; i < Enum.GetValues<BitPieceType>().Length; i++)
        {
            BitPieceType pieceType = (BitPieceType)i;
            if (pieceType.IsWhite())
            {
                WhitePieces |= Bitboards[i];
            }
            else if (pieceType.IsBlack())
            {
                BlackPieces |= Bitboards[i];
            }
            else
            {
                NeutralPieces |= Bitboards[i];
            }
        }

        Occupancy = WhitePieces | BlackPieces | NeutralPieces;
    }

    public static BitBoard FromPieces(Dictionary<AlgebraicPoint, Piece> pieces)
    {
        UInt128[] bitboards = new UInt128[Enum.GetValues<BitPieceType>().Length];
        UInt128 hasMoved = 0;

        foreach (var (point, piece) in pieces)
        {
            BitPieceType type = BitPieceMap.FromPiece(piece.Type, piece.Color);
            bitboards[(int)type] |= UInt128.One << point.AsIdx();

            if (piece.HasMoved)
            {
                hasMoved |= UInt128.One << point.AsIdx();
            }
        }

        return new BitBoard(bitboards, hasMoved);
    }

    public readonly ref UInt128 BitboardFor(BitPieceType pieceType) =>
        ref Bitboards[(int)pieceType];

    public readonly bool HasPieceMoved(byte position) =>
        (HasMoved & (UInt128.One << position)) != 0;

    public readonly bool HasPieceMoved(UInt128 mask) => (HasMoved & mask) != 0;

    public readonly UInt128 BitboardForFriendOf(GameColor color)
    {
        if (color is GameColor.White)
        {
            return WhitePieces;
        }
        else
        {
            return BlackPieces;
        }
    }

    public readonly UInt128 BitboardForEnemyOf(GameColor color)
    {
        if (color is GameColor.White)
        {
            return BlackPieces | NeutralPieces;
        }
        else
        {
            return WhitePieces | NeutralPieces;
        }
    }
}
