using System.Runtime.CompilerServices;
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
        UInt128 underagePawnsBitboard =
            board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White)
            | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black);

        ComputeBounces(
            board,
            pieceType,
            color,
            origin: position,
            bounceFrom: position,
            underagePawnsBitboard: underagePawnsBitboard,
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
        byte bounceFrom,
        UInt128 underagePawnsBitboard,
        ref UInt128 visitedMask,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = MagicLibrary.GetAttacks(
            MagicLibrary.BishopTable,
            bounceFrom,
            board.Occupancy
        );
        attacks &= ~visitedMask;
        AddUnderagePawnCapture(
            board,
            origin,
            pieceType,
            attacks: ref attacks,
            underagePawnsBitboard: underagePawnsBitboard,
            moves,
            ref moveCount
        );

        attacks &= ~board.BitboardForFriendOf(color);
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
                bounceFrom: edgeSquare,
                underagePawnsBitboard: underagePawnsBitboard,
                ref visitedMask,
                moves,
                ref moveCount
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddUnderagePawnCapture(
        BitBoard board,
        byte origin,
        PieceType pieceType,
        ref UInt128 attacks,
        UInt128 underagePawnsBitboard,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 underagePawnCapture = attacks & underagePawnsBitboard;
        attacks &= ~underagePawnsBitboard;
        while (underagePawnCapture != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref underagePawnCapture);
            if (board.TryGetPieceAt(toSquare, out var capturePiece))
            {
                BitMove move = new()
                {
                    From = origin,
                    To = toSquare,
                    Piece = pieceType,
                    ForcedMovePriority = ForcedMovePriority.UnderagePawn,
                };
                move.AddCapture(toSquare, capturePiece.Value.PieceType, capturePiece.Value.Color);
                moves[moveCount++] = move;
            }
        }
    }
}
