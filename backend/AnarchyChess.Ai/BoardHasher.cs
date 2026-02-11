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

    private static readonly ulong[,,] ZobristTable;
    private static readonly ulong ZobristSideToMove;

    private const int SquareCount = 10 * 10;

    static BoardHasher()
    {
        Random rng = new(6969);

        ZobristSideToMove = GenerateHash(rng);
        ZobristTable = new ulong[PieceTypeCount, ColorCount, SquareCount];

        for (int pieceType = 0; pieceType < PieceTypeCount; pieceType++)
        {
            for (int color = 0; color < ColorCount; color++)
            {
                for (int square = 0; square < SquareCount; square++)
                {
                    ZobristTable[pieceType, color, square] = GenerateHash(rng);
                }
            }
        }
    }

    private static ulong GenerateHash(Random rng)
    {
        return ((ulong)rng.Next() << 32) | (uint)rng.Next();
    }

    public UInt128 CalculateHash(BitBoard board)
    {
        UInt128 hash = 0;
        for (byte square = 0; square < SquareCount; square++)
        {
            if (board.TryGetPieceAt(square, out var piece))
            {
                int pieceTypeIdx = (int)piece.Value.Type;
                int colorIdx = (int)piece.Value.Color;
                hash ^= ZobristTable[pieceTypeIdx, colorIdx, square];
            }
        }
        if (board.IsWhiteToMove)
        {
            hash ^= ZobristSideToMove;
        }

        return hash;
    }
}
