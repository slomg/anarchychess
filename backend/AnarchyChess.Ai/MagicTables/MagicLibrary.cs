using AnarchyChess.EngineShared;
using MessagePack;

namespace AnarchyChess.Ai.MagicTables;

public static class MagicLibrary
{
    public static MagicPieceTable RookTable { get; }
    public static MagicPieceTable BishopTable { get; }
    public static MagicPieceTable TwoStraightSquaresTable { get; }

    public static MagicPieceTable LeftWhiteEnPassantTable { get; }
    public static MagicPieceTable LeftBlackEnPassantTable { get; }
    public static MagicPieceTable RightWhiteEnPassantTable { get; }
    public static MagicPieceTable RightBlackEnPassantTable { get; }

    private static readonly string _magicBasePath = Path.Combine(
        AppContext.BaseDirectory,
        "MagicTables"
    );

    static MagicLibrary()
    {
        RookTable = LoadTable("RookMagic.msgpack");
        BishopTable = LoadTable("BishopMagic.msgpack");
        TwoStraightSquaresTable = LoadTable("TwoStraightSquaresMagic.msgpack");

        LeftWhiteEnPassantTable = LoadTable("LeftWhiteEnPassantMagic.msgpack");
        LeftBlackEnPassantTable = LoadTable("LeftBlackEnPassantMagic.msgpack");
        RightWhiteEnPassantTable = LoadTable("RightWhiteEnPassantMagic.msgpack");
        RightBlackEnPassantTable = LoadTable("RightBlackEnPassantMagic.msgpack");
    }

    private static MagicPieceTable LoadTable(string fileName)
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(_magicBasePath, fileName));
        return MessagePackSerializer.Deserialize<MagicPieceTable>(bytes);
    }

    public static UInt128 GetAttacks(MagicPieceTable table, int square, UInt128 occupancy)
    {
        UInt128 blockers = occupancy & table.Masks[square];
        UInt128 index = (blockers * table.MagicNumbers[square]) >> table.Shifts[square];
        return table.AttackTable[square][(int)index];
    }
}
