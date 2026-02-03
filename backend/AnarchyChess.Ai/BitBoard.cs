namespace AnarchyChess.Ai;

public struct BitBoard
{
    public UInt128[,] Bitboards = new UInt128[
        Enum.GetValues<BitColor>().Length,
        Enum.GetValues<BitPiece>().Length
    ];
    public UInt128[] NeutralBitboards = new UInt128[Enum.GetValues<NeutralBitPiece>().Length];

    public UInt128 WhitePieces;
    public UInt128 BlackPieces;
    public UInt128 NeutralPieces;
    public UInt128 Occupancy;

    public UInt128 HasMoved;

    public BitBoard(UInt128[,] bitboards)
    {
        Bitboards = bitboards;

        for (int i = 0; i < Enum.GetValues<BitPiece>().Length; i++)
        {
            WhitePieces |= Bitboards[(int)BitColor.White, i];
            BlackPieces |= Bitboards[(int)BitColor.Black, i];
        }

        for (int i = 0; i < NeutralBitboards.Length; i++)
        {
            NeutralPieces |= NeutralBitboards[i];
        }

        Occupancy = WhitePieces | BlackPieces | NeutralPieces;
    }

    public readonly ref UInt128 BitboardFor(BitPiece pieceType, BitColor color) =>
        ref Bitboards[(int)color, (int)pieceType];

    public readonly ref UInt128 BitboardFor(NeutralBitPiece pieceType) =>
        ref NeutralBitboards[(int)pieceType];

    public readonly UInt128 BitboardForFriendOf(BitColor color)
    {
        if (color is BitColor.White)
        {
            return WhitePieces;
        }
        else
        {
            return BlackPieces;
        }
    }

    public readonly UInt128 BitboardForEnemyOf(BitColor color)
    {
        if (color is BitColor.White)
        {
            return BlackPieces | NeutralPieces;
        }
        else
        {
            return WhitePieces | NeutralPieces;
        }
    }
}
