using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Ai;

public class BitBoard
{
    public UInt128[,] Bitboards { get; private set; }

    public UInt128 WhitePieces { get; private set; }
    public UInt128 BlackPieces { get; private set; }
    public UInt128 NeutralPieces { get; private set; }
    public UInt128 Occupancy { get; private set; }

    public UInt128 HasMoved { get; private set; }

    public UInt128 WhiteEnemy { get; private set; }
    public UInt128 BlackEnemy { get; private set; }

    public BitBoard(UInt128[,]? bitboards = null, UInt128? hasMoved = null)
    {
        Bitboards =
            bitboards
            ?? new UInt128[
                Enum.GetValues<BitPieceColor>().Length,
                Enum.GetValues<PieceType>().Length
            ];
        HasMoved = hasMoved ?? 0;

        for (int i = 0; i < Enum.GetValues<PieceType>().Length; i++)
        {
            WhitePieces |= Bitboards[(int)BitPieceColor.White, i];
            BlackPieces |= Bitboards[(int)BitPieceColor.Black, i];
            NeutralPieces |= Bitboards[(int)BitPieceColor.Neutral, i];
        }

        Occupancy = WhitePieces | BlackPieces | NeutralPieces;

        WhiteEnemy = BlackPieces | NeutralPieces;
        BlackEnemy = WhitePieces | NeutralPieces;
    }

    public static BitBoard FromPieces(Dictionary<AlgebraicPoint, Piece> pieces)
    {
        UInt128[,] bitboards = new UInt128[
            Enum.GetValues<BitPieceColor>().Length,
            Enum.GetValues<PieceType>().Length
        ];
        UInt128 hasMoved = 0;

        foreach (var (point, piece) in pieces)
        {
            BitPieceColor color = piece.Color.Match(
                whenWhite: BitPieceColor.White,
                whenBlack: BitPieceColor.Black,
                whenNeutral: BitPieceColor.Neutral
            );
            bitboards[(int)color, (int)piece.Type] |= UInt128.One << point.AsIdx();

            if (piece.HasMoved)
            {
                hasMoved |= UInt128.One << point.AsIdx();
            }
        }

        return new BitBoard(bitboards, hasMoved);
    }

    public ref UInt128 BitboardFor(PieceType pieceType, BitPieceColor color) =>
        ref Bitboards[(int)color, (int)pieceType];

    public bool HasPieceMoved(byte position) => (HasMoved & (UInt128.One << position)) != 0;

    public bool HasPieceMoved(UInt128 mask) => (HasMoved & mask) != 0;

    public UInt128 BitboardForFriendOf(BitPieceColor color) =>
        color switch
        {
            BitPieceColor.White => WhitePieces,
            BitPieceColor.Black => BlackPieces,
            _ => 0,
        };

    public UInt128 BitboardForEnemyOf(BitPieceColor color) =>
        color switch
        {
            BitPieceColor.White => WhiteEnemy,
            BitPieceColor.Black => BlackEnemy,
            _ => 0,
        };
}
