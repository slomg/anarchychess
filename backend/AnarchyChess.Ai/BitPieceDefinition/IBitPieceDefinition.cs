namespace AnarchyChess.Ai.BitPieceDefinition;

public interface IBitPieceDefinition
{
    void GenerateMoves(
        BitBoard board,
        BitPieceType pieceType,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    );
}
