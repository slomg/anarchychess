using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public interface IBitPieceDefinition
{
    void GenerateMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    );
}
