using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public interface IBitPieceDefinition
{
    BitPiece PieceType { get; }

    void GenerateMoves(
        BitBoard board,
        GameColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    );
}
