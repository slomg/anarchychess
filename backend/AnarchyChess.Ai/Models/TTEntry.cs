namespace AnarchyChess.Ai.Models;

public enum TTNodeType
{
    Exact,
    Alpha,
    Beta,
}

public struct TTEntry
{
    public UInt128 Key;
    public float Score;
    public int Depth;
    public TTNodeType NodeType;
    public BitMove? BestMove;
}
