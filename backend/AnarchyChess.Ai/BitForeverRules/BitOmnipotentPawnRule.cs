using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitForeverRules;

public sealed class BitOmnipotentPawnRule : IBitForeverRule
{
    public void GenerateMoves(BitBoard board, Span<BitMove> moves, ref int moveCount)
    {
        if (!board.CanSpawnOmnipotentPawn)
        {
            return;
        }

        byte square = board.IsWhiteToMove
            ? GameLogicConstants.WhiteOmnipotentPawnIdx
            : GameLogicConstants.BlackOmnipotentPawnIdx;
        UInt128 captureMask = board.IsWhiteToMove
            ? GameLogicConstants.WhiteOmnipotentPawnMask
            : GameLogicConstants.BlackOmnipotentPawnMask;
        BitPieceColor color = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;
        moves[moveCount++] = new BitMove()
        {
            From = square,
            To = square,
            Piece = new() { Type = PieceType.Pawn, Color = color },
            CapturesMask = captureMask,
            SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
        };
    }
}
