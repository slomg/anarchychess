using System.Runtime.CompilerServices;
using AnarchyChess.Ai.BitForeverRules;
using AnarchyChess.Ai.BitPieceDefinition;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public static class BitMoveGenerator
{
    private static readonly IBitPieceDefinition[] _pieceDefinitions =
    [
        new BitKingDefinition(),
        new BitQueenDefinition(),
        new BitPawnDefinition(),
        new BitRookDefinition(),
        new BitBishopDefinition(),
        new BitHorseyDefinition(),
        new BitKnookDefinition(),
        new BitAntiqueenDefinition(),
        new BitUnderagePawnDefinition(),
        new BitSterilePawnDefinition(),
        new BitTraitorRookDefinition(),
        new BitCheckerDefinition(),
    ];

    private static readonly IBitForeverRule[] _foreverRules = [new BitOmnipotentPawnRule()];

    public static void Generate(
        BitBoard board,
        Span<BitMove> moves,
        ref int moveCount,
        int depth = EngineConstants.MaxDepth,
        int maxDepth = EngineConstants.MaxDepth
    )
    {
        BitPieceColor color = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;
        UInt128 seenThrows = 0;

        for (int pieceTypeIdx = 0; pieceTypeIdx < board.Bitboards.GetLength(1); pieceTypeIdx++)
        {
            PieceType pieceType = (PieceType)pieceTypeIdx;
            IBitPieceDefinition definition = _pieceDefinitions[pieceTypeIdx];

            UInt128 colorBitboard = board.BitboardFor(pieceType, color);
            GenerateForPieces(
                board,
                colorBitboard,
                definition,
                piece: new BitPiece() { Type = pieceType, Color = color },
                seenThrows: ref seenThrows,
                depth: depth,
                maxDepth: maxDepth,
                moves,
                ref moveCount
            );

            UInt128 neutralBitboard = board.BitboardFor(pieceType, BitPieceColor.Neutral);
            GenerateForPieces(
                board,
                neutralBitboard,
                definition,
                piece: new BitPiece() { Type = pieceType, Color = BitPieceColor.Neutral },
                seenThrows: ref seenThrows,
                depth: depth,
                maxDepth: maxDepth,
                moves,
                ref moveCount
            );
        }

        for (int i = 0; i < _foreverRules.Length; i++)
        {
            _foreverRules[i].GenerateMoves(board, moves, ref moveCount);
        }

        int newMoveCount = 0;
        ForcedMovePriority highestPriority = ForcedMovePriority.None;
        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            if (move.ForcedMovePriority > highestPriority)
            {
                highestPriority = move.ForcedMovePriority;
                newMoveCount = 0;
            }

            if (move.ForcedMovePriority == highestPriority)
            {
                moves[newMoveCount++] = move;
            }
        }

        moveCount = newMoveCount;
    }

    public static void GenerateForPiece(
        BitBoard board,
        byte position,
        BitPiece piece,
        Span<BitMove> moves,
        ref int moveCount,
        int depth = EngineConstants.MaxDepth,
        int maxDepth = EngineConstants.MaxDepth
    )
    {
        UInt128 bitboard = board.BitboardFor(piece.Type, piece.Color);

        IBitPieceDefinition definition = _pieceDefinitions[(int)piece.Type];
        if ((bitboard & (UInt128.One << position)) != 0)
        {
            UInt128 seenThrows = 0;
            definition.GenerateMoves(
                board,
                piece,
                position,
                seenThrows: ref seenThrows,
                depth: depth,
                maxDepth: maxDepth,
                moves: moves,
                moveCount: ref moveCount
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateForPieces(
        BitBoard board,
        UInt128 bitboard,
        IBitPieceDefinition definition,
        BitPiece piece,
        ref UInt128 seenThrows,
        int depth,
        int maxDepth,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        while (bitboard != 0)
        {
            int squareIndex = BitboardHelpers.BitScanForward(ref bitboard);
            byte position = (byte)squareIndex;

            if ((board.StunnedPieces & (UInt128.One << position)) != 0)
            {
                return;
            }

            definition.GenerateMoves(
                board,
                piece,
                position,
                seenThrows: ref seenThrows,
                depth: depth,
                maxDepth: maxDepth,
                moves,
                ref moveCount
            );
        }
    }
}
