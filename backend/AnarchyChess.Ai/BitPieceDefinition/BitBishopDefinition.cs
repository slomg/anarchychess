using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitBishopDefinition : IBitPieceDefinition
{
    public void GenerateMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 visitedMask = 0;
        ComputeBounces(
            board,
            pieceType,
            color,
            origin: position,
            jumpFrom: position,
            ref visitedMask,
            moves,
            ref moveCount
        );
    }

    private static void ComputeBounces(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte origin,
        byte jumpFrom,
        ref UInt128 visitedMask,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = MagicLibrary.GetAttacks(
            MagicLibrary.BishopTable,
            jumpFrom,
            board.Occupancy
        );
        attacks &= ~board.BitboardForFriendOf(color);
        attacks &= ~visitedMask;

        if (attacks == 0)
        {
            return;
        }

        visitedMask |= attacks;
        UInt128 edges = attacks & BitboardConstants.EdgeMasks & ~board.Occupancy;

        BitboardHelpers.CreateMoveFromAttacks(
            origin,
            pieceType,
            board,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );

        while (edges != 0)
        {
            byte edgeSquare = (byte)BitboardHelpers.BitScanForward(ref edges);
            ComputeBounces(
                board,
                pieceType,
                color,
                origin: origin,
                jumpFrom: edgeSquare,
                ref visitedMask,
                moves,
                ref moveCount
            );
        }
    }
}
