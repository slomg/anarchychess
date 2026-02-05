using System.Runtime.CompilerServices;
using AnarchyChess.Ai.BitPieceDefinition;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IBitMovesGenerator
{
    void Generate(BitBoard board, Span<BitMove> moves, ref int moveCount);
    void GenerateForPiece(
        BitBoard board,
        byte position,
        PieceType pieceType,
        BitPieceColor color,
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
    };

    public void Generate(BitBoard board, Span<BitMove> moves, ref int moveCount)
    {
        for (int colorIdx = 0; colorIdx < board.Bitboards.GetLength(0); colorIdx++)
        {
            BitPieceColor color = (BitPieceColor)colorIdx;

            for (int pieceTypeIdx = 0; pieceTypeIdx < board.Bitboards.GetLength(1); pieceTypeIdx++)
            {
                PieceType pieceType = (PieceType)pieceTypeIdx;
                UInt128 bitboard = board.BitboardFor(pieceType, color);

                if (_pieceDefinitions.TryGetValue(pieceType, out var definition))
                {
                    GenerateForPieces(
                        board,
                        bitboard,
                        definition,
                        pieceType,
                        color,
                        moves,
                        ref moveCount
                    );
                }
            }
        }
    }

    public void GenerateForPiece(
        BitBoard board,
        byte position,
        PieceType pieceType,
        BitPieceColor color,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 bitboard = board.BitboardFor(pieceType, color);
        if (
            _pieceDefinitions.TryGetValue(pieceType, out var definition)
            && (bitboard & (UInt128.One << position)) != 0
        )
        {
            definition.GenerateMoves(board, pieceType, color, position, moves, ref moveCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateForPieces(
        BitBoard board,
        UInt128 bitboard,
        IBitPieceDefinition definition,
        PieceType pieceType,
        BitPieceColor color,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        while (bitboard != 0)
        {
            int squareIndex = BitboardHelpers.BitScanForward(ref bitboard);
            byte position = (byte)squareIndex;

            definition.GenerateMoves(board, pieceType, color, position, moves, ref moveCount);
        }
    }
}
