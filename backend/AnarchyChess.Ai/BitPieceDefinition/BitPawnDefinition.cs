using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitPawnDefinition : IBitPieceDefinition
{
    private static readonly BitPawnLikeDefinition PawnLikeDefinition = new(
        promotesTo: [.. GameLogicConstants.PromotablePieces],
        maxInitialSteps: 3
    );

    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    ) => PawnLikeDefinition.GenerateMoves(board, piece, position, moves, ref moveCount);
}
