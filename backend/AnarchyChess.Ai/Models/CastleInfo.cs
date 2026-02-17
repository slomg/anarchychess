using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.Models;

public struct CastleInfo
{
    public byte KingStart;
    public byte RookStart;
    public byte KingDest;
    public byte RookDest;
    public UInt128 BetweenMask;
    public SpecialMoveType MoveType;
}
