using System.Runtime.CompilerServices;
using AnarchyChess.Ai.BitPieceDefinition;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public interface IBitMovesGenerator
{
    void Generate(BitBoard board, Span<BitMove> moves, ref int movesCount);
    void GenerateForPiece(
        BitBoard board,
        BitPieceType pieceType,
        GameColor color,
        Span<BitMove> moves,
        ref int movesCount
    );
    void GenerateForPiece(
        BitBoard board,
        NeutralBitPieceType pieceType,
        Span<BitMove> moves,
        ref int movesCount
    );
}

public sealed class BitMovesGenerator : IBitMovesGenerator
{
    private readonly Dictionary<BitPieceType, IBitPieceDefinition> _pieceDefinitions = new()
    {
        [BitPieceType.King] = new BitKingDefinition(),
    };

    private readonly Dictionary<
        NeutralBitPieceType,
        IBitPieceDefinition
    > _neutralPieceDefinitions = [];

    public void Generate(BitBoard board, Span<BitMove> moves, ref int movesCount)
    {
        for (int colorIdx = 0; colorIdx < board.Bitboards.GetLength(0); colorIdx++)
        {
            GameColor color = (GameColor)colorIdx;

            for (int pieceTypeIdx = 0; pieceTypeIdx < board.Bitboards.GetLength(1); pieceTypeIdx++)
            {
                BitPieceType pieceType = (BitPieceType)pieceTypeIdx;
                UInt128 bitboard = board.BitboardFor(pieceType, color);
                if (_pieceDefinitions.TryGetValue(pieceType, out var definition))
                {
                    GenerateForPieces(board, bitboard, definition, color, moves, ref movesCount);
                }
            }
        }

        for (
            int neutralPieceTypeIdx = 0;
            neutralPieceTypeIdx < board.NeutralBitboards.Length;
            neutralPieceTypeIdx++
        )
        {
            NeutralBitPieceType neutralBitPieceType = (NeutralBitPieceType)neutralPieceTypeIdx;
            UInt128 bitboard = board.NeutralBitboards[neutralPieceTypeIdx];
            if (_neutralPieceDefinitions.TryGetValue(neutralBitPieceType, out var definition))
            {
                GenerateForPieces(board, bitboard, definition, color: null, moves, ref movesCount);
            }
        }
    }

    public void GenerateForPiece(
        BitBoard board,
        BitPieceType pieceType,
        GameColor color,
        Span<BitMove> moves,
        ref int movesCount
    )
    {
        UInt128 bitboard = board.BitboardFor(pieceType, color);
        if (_pieceDefinitions.TryGetValue(pieceType, out var definition))
        {
            GenerateForPieces(board, bitboard, definition, color, moves, ref movesCount);
        }
    }

    public void GenerateForPiece(
        BitBoard board,
        NeutralBitPieceType pieceType,
        Span<BitMove> moves,
        ref int movesCount
    )
    {
        UInt128 bitboard = board.BitboardFor(pieceType);
        if (_neutralPieceDefinitions.TryGetValue(pieceType, out var definition))
        {
            GenerateForPieces(board, bitboard, definition, color: null, moves, ref movesCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GenerateForPieces(
        BitBoard board,
        UInt128 bitboard,
        IBitPieceDefinition definition,
        GameColor? color,
        Span<BitMove> moves,
        ref int movesCount
    )
    {
        while (bitboard != 0)
        {
            int squareIndex = BitboardHelpers.BitScanForward(ref bitboard);
            byte position = (byte)squareIndex;

            definition.GenerateMoves(board, color, position, moves, ref movesCount);
        }
    }
}
