using System.Runtime.CompilerServices;
using AnarchyChess.Ai.BitPieceDefinition;

namespace AnarchyChess.Ai;

public interface IBitMovesGenerator
{
    void Generate(BitBoard board, Span<BitMove> moves, ref int movesCount);
    void GenerateForPiece(
        BitBoard board,
        BitPieceType pieceType,
        Span<BitMove> moves,
        ref int movesCount
    );
}

public sealed class BitMovesGenerator : IBitMovesGenerator
{
    private readonly Dictionary<BitPieceType, IBitPieceDefinition> _pieceDefinitions = new()
    {
        [BitPieceType.WhiteKing] = new BitKingDefinition(),
        [BitPieceType.BlackKing] = new BitKingDefinition(),
    };

    public void Generate(BitBoard board, Span<BitMove> moves, ref int movesCount)
    {
        for (int pieceTypeIdx = 0; pieceTypeIdx < board.Bitboards.Length; pieceTypeIdx++)
        {
            BitPieceType pieceType = (BitPieceType)pieceTypeIdx;
            UInt128 bitboard = board.BitboardFor(pieceType);
            if (_pieceDefinitions.TryGetValue(pieceType, out var definition))
            {
                GenerateForPieces(board, bitboard, definition, pieceType, moves, ref movesCount);
            }
        }
    }

    public void GenerateForPiece(
        BitBoard board,
        BitPieceType pieceType,
        Span<BitMove> moves,
        ref int movesCount
    )
    {
        UInt128 bitboard = board.BitboardFor(pieceType);
        if (_pieceDefinitions.TryGetValue(pieceType, out var definition))
        {
            GenerateForPieces(board, bitboard, definition, pieceType, moves, ref movesCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateForPieces(
        BitBoard board,
        UInt128 bitboard,
        IBitPieceDefinition definition,
        BitPieceType pieceType,
        Span<BitMove> moves,
        ref int movesCount
    )
    {
        while (bitboard != 0)
        {
            int squareIndex = BitboardHelpers.BitScanForward(ref bitboard);
            byte position = (byte)squareIndex;

            definition.GenerateMoves(board, pieceType, position, moves, ref movesCount);
        }
    }
}
