using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitForeverRules;

public sealed class BitOmnipotentPawnRule : IBitForeverRule
{
    public static readonly byte WhiteSquare = new AlgebraicPoint("h3").AsIdx();
    public static readonly UInt128 WhiteSquareMask = UInt128.One << WhiteSquare;

    public static readonly byte BlackSquare = new AlgebraicPoint("h8").AsIdx();
    public static readonly UInt128 BlackSquareMask = UInt128.One << BlackSquare;

    public void GenerateMoves(BitBoard board, Span<BitMove> moves, ref int moveCount)
    {
        if (board.LastCaptureMask == 0)
        {
            return;
        }

        if (board.IsWhiteToMove && (board.LastCaptureMask & WhiteSquareMask) != 0)
        {
            moves[moveCount++] = new BitMove()
            {
                From = WhiteSquare,
                To = WhiteSquare,
                Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
                CapturesMask = WhiteSquareMask,
                SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
            };
        }
        else if (!board.IsWhiteToMove && (board.LastCaptureMask & BlackSquareMask) != 0)
        {
            moves[moveCount++] = new BitMove()
            {
                From = BlackSquare,
                To = BlackSquare,
                Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
                CapturesMask = BlackSquareMask,
                SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
            };
        }
    }
}
