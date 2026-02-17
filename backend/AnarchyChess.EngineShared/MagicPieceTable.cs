using MessagePack;

namespace AnarchyChess.EngineShared;

[MessagePackObject]
public class MagicPieceTable
{
    [Key(0)]
    public required UInt128[] Masks { get; set; }

    [Key(1)]
    public required UInt128[] MagicNumbers { get; set; }

    [Key(2)]
    public required int[] Shifts { get; set; }

    [Key(3)]
    public required UInt128[][] AttackTable { get; set; }
}
