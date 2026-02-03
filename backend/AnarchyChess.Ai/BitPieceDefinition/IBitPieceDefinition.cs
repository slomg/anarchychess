namespace AnarchyChess.Ai.BitPieceDefinition;

public interface IBitPieceDefinition
{
    BitPiece PieceType { get; }

    void GenerateMoves(
        BitBoard board,
        BitColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    );
}
