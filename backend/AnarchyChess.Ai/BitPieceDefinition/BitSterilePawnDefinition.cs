using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitSterilePawnDefinition : IBitPieceDefinition
{
    private static readonly BitPawnLikeDefinition PawnLikeDefinition = new(
        promotesTo: [.. GameLogicConstants.PromotablePieces.Where(x => x is not PieceType.Queen)],
        maxInitialSteps: 1
    );

    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    ) => PawnLikeDefinition.GenerateMoves(board, piece, position, moves, ref moveCount);
}
