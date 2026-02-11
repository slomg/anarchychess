using System.Runtime.CompilerServices;
using AnarchyChess.Ai.BitPieceDefinition;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IBitMovesGenerator
{
    void Generate(
        BitBoard board,
        Span<BitMove> moves,
        ref int moveCount,
        Span<int> moveCountByPiece
    );
    void GenerateForPiece(
        BitBoard board,
        byte position,
        BitPiece piece,
        Span<BitMove> moves,
        ref int moveCount
    );
}

public sealed class BitMovesGenerator : IBitMovesGenerator
{
    private readonly Dictionary<PieceType, IBitPieceDefinition> _pieceDefinitions = new()
    {
        [PieceType.King] = new BitKingDefinition(),
        [PieceType.Queen] = new BitQueenDefinition(),
        [PieceType.Rook] = new BitRookDefinition(),
        [PieceType.Bishop] = new BitBishopDefinition(),
        [PieceType.Horsey] = new BitHorseyDefinition(),
        [PieceType.Pawn] = new BitPawnDefinition(),

        [PieceType.Knook] = new BitKnookDefinition(),
        [PieceType.Antiqueen] = new BitAntiqueenDefinition(),
        [PieceType.UnderagePawn] = new BitUnderagePawnDefinition(),
        [PieceType.SterilePawn] = new BitSterilePawnDefinition(),
        [PieceType.TraitorRook] = new BitTraitorRookDefinition(),
        [PieceType.Checker] = new BitCheckerDefinition(),
    };

    public void Generate(
        BitBoard board,
        Span<BitMove> moves,
        ref int moveCount,
        Span<int> moveCountByPiece
    )
    {
        BitPieceColor color = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;

        for (int pieceTypeIdx = 0; pieceTypeIdx < board.Bitboards.GetLength(1); pieceTypeIdx++)
        {
            PieceType pieceType = (PieceType)pieceTypeIdx;
            if (!_pieceDefinitions.TryGetValue(pieceType, out var definition))
            {
                continue;
            }

            UInt128 colorBitboard = board.BitboardFor(pieceType, color);
            GenerateForPieces(
                board,
                colorBitboard,
                definition,
                piece: new BitPiece() { Type = pieceType, Color = color },
                moves,
                ref moveCount
            );

            UInt128 neutralBitboard = board.BitboardFor(pieceType, BitPieceColor.Neutral);
            GenerateForPieces(
                board,
                neutralBitboard,
                definition,
                piece: new BitPiece() { Type = pieceType, Color = BitPieceColor.Neutral },
                moves,
                ref moveCount
            );
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

            moveCountByPiece[(int)move.Piece.Type]++;
        }

        moveCount = newMoveCount;
    }

    public void GenerateForPiece(
        BitBoard board,
        byte position,
        BitPiece piece,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 bitboard = board.BitboardFor(piece.Type, piece.Color);
        if (
            _pieceDefinitions.TryGetValue(piece.Type, out var definition)
            && (bitboard & (UInt128.One << position)) != 0
        )
        {
            definition.GenerateMoves(board, piece, position, moves, ref moveCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateForPieces(
        BitBoard board,
        UInt128 bitboard,
        IBitPieceDefinition definition,
        BitPiece piece,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        while (bitboard != 0)
        {
            int squareIndex = BitboardHelpers.BitScanForward(ref bitboard);
            byte position = (byte)squareIndex;

            definition.GenerateMoves(board, piece, position, moves, ref moveCount);
        }
    }
}
