using AnarchyChess.EngineShared;
using ProtoBuf;

namespace AnarchyChess.Ai.Models;

[ProtoContract]
public struct BitPiece
{
    [ProtoMember(1)]
    public PieceType Type;

    [ProtoMember(2)]
    public BitPieceColor Color;
}
