using System.Runtime.CompilerServices;
using AnarchyChess.EngineShared;
using MessagePack;

namespace AnarchyChess.Ai.MagicTables;

public static class MagicLibrary
{
    public static MagicPieceTable RookTable { get; }
    public static MagicPieceTable BishopTable { get; }

    public static MagicPieceTable TwoStraightSquaresTable { get; }
    public static MagicPieceTable TwoDiagonalSquaresTable { get; }

    public static MagicPieceTable WhiteLeftEnPassantTable { get; }
    public static MagicPieceTable WhiteRightEnPassantTable { get; }

    public static MagicPieceTable BlackLeftEnPassantTable { get; }
    public static MagicPieceTable BlackRightEnPassantTable { get; }

    private static readonly string _magicBasePath = Path.Combine(
        AppContext.BaseDirectory,
        "MagicTables"
    );

    static MagicLibrary()
    {
        RookTable = LoadTable("RookMagic.msgpack");
        BishopTable = LoadTable("BishopMagic.msgpack");

        TwoStraightSquaresTable = LoadTable("TwoStraightSquaresMagic.msgpack");
        TwoDiagonalSquaresTable = LoadTable("TwoDiagonalSquaresMagic.msgpack");

        WhiteLeftEnPassantTable = LoadTable("WhiteLeftEnPassantMagic.msgpack");
        WhiteRightEnPassantTable = LoadTable("WhiteRightEnPassantMagic.msgpack");

        BlackLeftEnPassantTable = LoadTable("BlackLeftEnPassantMagic.msgpack");
        BlackRightEnPassantTable = LoadTable("BlackRightEnPassantMagic.msgpack");
    }

    private static MagicPieceTable LoadTable(string fileName)
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(_magicBasePath, fileName));
        return MessagePackSerializer.Deserialize<MagicPieceTable>(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 GetAttacks(MagicPieceTable table, int square, UInt128 occupancy)
    {
        UInt128 blockers = occupancy & table.Masks[square];
        UInt128 index = (blockers * table.MagicNumbers[square]) >> table.Shifts[square];
        return table.AttackTable[square][(int)index];
    }
}
