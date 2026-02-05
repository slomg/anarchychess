using AnarchyChess.EngineShared;
using MessagePack;

namespace AnarchyChess.Ai.MagicTables;

public static class MagicLibrary
{
    public static MagicPieceTable RookTable { get; }
    public static MagicPieceTable QueenDiagonalTable { get; }

    private static readonly string _magicBasePath = Path.Combine(
        AppContext.BaseDirectory,
        "MagicTables"
    );

    static MagicLibrary()
    {
        RookTable = LoadTable("RookMagic.msgpack");
        QueenDiagonalTable = LoadTable("QueenDiagonal.msgpack");
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
