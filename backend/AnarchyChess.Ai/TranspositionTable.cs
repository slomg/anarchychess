using System.Runtime.CompilerServices;

namespace AnarchyChess.Ai;

public enum NodeType : byte
{
    Exact,
    LowerBound,
    UpperBound,
}

public struct TTEntry
{
    public ulong Key;
    public int Depth;
    public int Score;
    public NodeType Type;
    public int BestMove;
}

public sealed class TranspositionTable
{
    private readonly TTEntry[] _table;
    private readonly int _mask;

    public TranspositionTable(int sizeMb = 512)
    {
        int entrySize = Unsafe.SizeOf<TTEntry>();
        int entryCount = sizeMb * 1024 * 1024 / entrySize;

        int size = 1;
        while (size < entryCount)
        {
            size <<= 1;
        }

        _table = new TTEntry[size];
        _mask = size - 1;
    }

    public bool TryProbe(ulong key, int depth, int alpha, int beta, out int score, out int bestMove)
    {
        ref TTEntry entry = ref _table[key & (ulong)_mask];
        score = 0;
        bestMove = 0;

        if (entry.Key != key)
        {
            return false;
        }

        bestMove = entry.BestMove;
        if (entry.Depth < depth)
        {
            return false;
        }

        score = entry.Score;
        if (entry.Type == NodeType.Exact)
        {
            return true;
        }

        if (entry.Type == NodeType.LowerBound && score >= beta)
        {
            return true;
        }

        if (entry.Type == NodeType.UpperBound && score <= alpha)
        {
            return true;
        }

        return false;
    }

    public void Store(ulong key, int depth, int score, NodeType type, int bestMove)
    {
        int index = (int)(key & (ulong)_mask);

        TTEntry entry = _table[index];

        if (entry.Key == 0 || depth >= entry.Depth)
        {
            _table[index] = new()
            {
                Key = key,
                Depth = depth,
                Score = score,
                Type = type,
                BestMove = bestMove,
            };
        }
    }
}
