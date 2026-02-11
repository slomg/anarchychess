using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IBoardHasher
{
    UInt128 CalculateHash(BitBoard board);
}

public class BoardHasher : IBoardHasher
{
    private static readonly int PieceTypeCount = Enum.GetValues<PieceType>().Length;
    private static readonly int ColorCount = Enum.GetValues<BitPieceColor>().Length;

    private static readonly UInt128[,,] ZobristTable;
    private static readonly UInt128 ZobristSideToMove;

    private const int BitboardChunks = 2;

    static BoardHasher()
    {
        Random rng = new(6969);

        ZobristSideToMove = GenerateHash(rng);
        ZobristTable = new UInt128[PieceTypeCount, ColorCount, BitboardChunks];

        for (int pieceType = 0; pieceType < PieceTypeCount; pieceType++)
        {
            for (int color = 0; color < ColorCount; color++)
            {
                for (int chunk = 0; chunk < BitboardChunks; chunk++)
                {
                    ZobristTable[pieceType, color, chunk] = GenerateHash(rng);
                }
            }
        }
    }

    private static UInt128 GenerateHash(Random rng)
    {
        ulong low = (uint)rng.Next() | ((ulong)rng.Next() << 32);
        ulong high = (uint)rng.Next() | ((ulong)rng.Next() << 32);
        return new UInt128(low, high);
    }

    public UInt128 CalculateHash(BitBoard board)
    {
        UInt128 hash = 0;
        for (int pieceType = 0; pieceType < PieceTypeCount; pieceType++)
        {
            for (int color = 0; color < ColorCount; color++)
            {
                UInt128 bitboard = board.BitboardFor((PieceType)pieceType, (BitPieceColor)color);
                ulong low = (ulong)bitboard;
                ulong high = (ulong)(bitboard >> 64);

                hash ^= ZobristTable[pieceType, color, 0] * low;
                hash ^= ZobristTable[pieceType, color, 1] * high;
            }
        }
        if (board.IsWhiteToMove)
        {
            hash ^= ZobristSideToMove;
        }

        return hash;
    }
}
