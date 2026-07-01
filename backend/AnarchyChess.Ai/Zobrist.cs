using System.Runtime.CompilerServices;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public static class Zobrist
{
    public static ulong[,,] PieceSquare { get; }
    public static ulong SideToMove { get; }
    public static ulong[,] StunnedForPlies { get; }
    public static ulong[] HasMoved { get; }
    public static ulong[] EnPassantSquare { get; }
    public static ulong CanSpawnOmnipotentPawn { get; }

    private static readonly int PieceTypes = Enum.GetValues<PieceType>().Length;
    private static readonly int Colors = Enum.GetValues<BitPieceColor>().Length;

    static Zobrist()
    {
        Random rng = new(6969);

        PieceSquare = new ulong[PieceTypes, Colors, 100];
        for (int piece = 0; piece < PieceTypes; piece++)
        {
            for (int color = 0; color < Colors; color++)
            {
                for (int square = 0; square < 100; square++)
                {
                    PieceSquare[piece, color, square] = NextULong(rng);
                }
            }
        }

        SideToMove = NextULong(rng);
        CanSpawnOmnipotentPawn = NextULong(rng);

        StunnedForPlies = new ulong[100, EngineConstants.MaxStun];
        for (int square = 0; square < 100; square++)
        {
            for (int stun = 0; stun < EngineConstants.MaxStun; stun++)
            {
                StunnedForPlies[square, stun] = NextULong(rng);
            }
        }

        HasMoved = CreateSquareZobrist(rng);
        EnPassantSquare = CreateSquareZobrist(rng);
    }

    private static ulong[] CreateSquareZobrist(Random rng)
    {
        ulong[] hashes = new ulong[100];
        for (int square = 0; square < 100; square++)
        {
            hashes[square] = NextULong(rng);
        }
        return hashes;
    }

    private static ulong NextULong(Random rng)
    {
        byte[] bytes = new byte[8];
        rng.NextBytes(bytes);
        return BitConverter.ToUInt64(bytes, 0);
    }

    public static ulong Compute(BitBoard board)
    {
        ulong hash = 0;

        for (byte square = 0; square < 100; square++)
        {
            if (board.TryGetPieceAt(square, out var piece))
            {
                hash ^= PieceSquare[(int)piece.Value.Type, (int)piece.Value.Color, square];
            }
        }

        if (!board.IsWhiteToMove)
        {
            hash ^= SideToMove;
        }

        for (int square = 0; square < board.StunnedForPlies.Length; square++)
        {
            int stunnedForPlies = board.StunnedForPlies[square];
            if (stunnedForPlies > 0)
            {
                hash ^= StunnedForPlies[square, stunnedForPlies];
            }
        }

        if (board.EnPassantSquaresMask != 0)
        {
            hash ^= EnPassantSquare[board.EnPassantPawnSquare];
        }
        if (board.CanSpawnOmnipotentPawn)
        {
            hash ^= CanSpawnOmnipotentPawn;
        }
        hash ^= HashMask(board.HasMoved, HasMoved);

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong HashMask(UInt128 mask, ulong[] zobrist)
    {
        ulong hash = 0;
        while (mask != 0)
        {
            byte square = BitboardHelpers.BitScanForward(ref mask);
            hash ^= zobrist[square];
        }
        return hash;
    }
}
