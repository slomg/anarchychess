using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.BitForeverRules;

public interface IBitForeverRule
{
    void GenerateMoves(BitBoard board, Span<BitMove> moves, ref int moveCount);
}
