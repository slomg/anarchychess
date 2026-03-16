using ProtoBuf;

namespace AnarchyChess.Ai.Service.DTO;

[ProtoContract]
public struct UInt128Surrogate
{
    [ProtoMember(1)]
    public ulong High { get; set; }

    [ProtoMember(2)]
    public ulong Low { get; set; }

    public static implicit operator UInt128(UInt128Surrogate surrogate) =>
        new(surrogate.High, surrogate.Low);

    public static explicit operator UInt128Surrogate(UInt128 value) =>
        new() { High = (ulong)(value >> 64), Low = (ulong)(value & ulong.MaxValue) };
}
