namespace AnarchyChess.Ai.BitPieceDefinition;

public interface IBitPieceDefinition
{
    void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    );
}
