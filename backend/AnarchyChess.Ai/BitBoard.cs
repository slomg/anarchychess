using AnarchyChess.Ai.Constants;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public struct BitBoard
{
    public UInt128[,] Bitboards;
    public UInt128[] NeutralBitboards;

    public UInt128 WhitePieces;
    public UInt128 BlackPieces;
    public UInt128 NeutralPieces;
    public UInt128 Occupancy;

    public UInt128 HasMoved;

    public BitBoard(
        UInt128[,]? bitboards = null,
        UInt128[]? neutralBitboards = null,
        UInt128? hasMoved = null
    )
    {
        Bitboards =
            bitboards
            ?? new UInt128[Enum.GetValues<GameColor>().Length, Enum.GetValues<BitPiece>().Length];
        NeutralBitboards =
            neutralBitboards ?? new UInt128[Enum.GetValues<NeutralBitPiece>().Length];
        HasMoved = hasMoved ?? 0;

        for (int i = 0; i < Enum.GetValues<BitPiece>().Length; i++)
        {
            WhitePieces |= Bitboards[(int)GameColor.White, i];
            BlackPieces |= Bitboards[(int)GameColor.Black, i];
        }

        for (int i = 0; i < NeutralBitboards.Length; i++)
        {
            NeutralPieces |= NeutralBitboards[i];
        }

        Occupancy = WhitePieces | BlackPieces | NeutralPieces;
    }

    public static BitBoard FromPieces(Dictionary<AlgebraicPoint, Piece> pieces)
    {
        UInt128[,] bitboards = new UInt128[
            Enum.GetValues<GameColor>().Length,
            Enum.GetValues<BitPiece>().Length
        ];
        UInt128[] neutralBitboards = new UInt128[Enum.GetValues<NeutralBitPiece>().Length];
        UInt128 hasMoved = 0;

        foreach (var (point, piece) in pieces)
        {
            if (piece.Color is null)
            {
                NeutralBitPiece type = BitPieceMap.Neutral[piece.Type];
                neutralBitboards[(int)type] |= UInt128.One << point.AsIdx();
            }
            else
            {
                BitPiece type = BitPieceMap.Colored[piece.Type];
                bitboards[(int)piece.Color.Value, (int)type] |= UInt128.One << point.AsIdx();
            }

            if (piece.HasMoved)
            {
                hasMoved |= UInt128.One << point.AsIdx();
            }
        }

        return new BitBoard(bitboards, neutralBitboards, hasMoved);
    }

    public readonly ref UInt128 BitboardFor(BitPiece pieceType, GameColor color) =>
        ref Bitboards[(int)color, (int)pieceType];

    public readonly ref UInt128 BitboardFor(NeutralBitPiece pieceType) =>
        ref NeutralBitboards[(int)pieceType];

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
