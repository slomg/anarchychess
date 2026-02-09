using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitUnderagePawnDefinition : IBitPieceDefinition
{
    private static readonly BitPawnLikeDefinition PawnLikeDefinition = new(
        promotesTo: [.. GameLogicConstants.PromotablePieces],
        maxInitialSteps: 2
    );

    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    ) => PawnLikeDefinition.GenerateMoves(board, piece, position, moves, ref moveCount);
}
