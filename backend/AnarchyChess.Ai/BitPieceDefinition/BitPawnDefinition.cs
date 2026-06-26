using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitPawnDefinition : IBitPieceDefinition
{
    private static readonly BitPawnLikeDefinition PawnLikeDefinition = new(
        promotesTo: [.. GameLogicConstants.PromotablePieces, PieceType.Pawn],
        maxInitialSteps: 3
    );

    public void GenerateMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        ref UInt128 seenThrows,
        int depth,
        int maxDepth,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        PawnLikeDefinition.GenerateMoves(board, piece, position, moves, ref moveCount);
        if (depth > maxDepth - 4)
        {
            GenerateThrowMoves(board, piece, position, ref seenThrows, moves, ref moveCount);
        }
    }

    private static void GenerateThrowMoves(
        BitBoard board,
        BitPiece piece,
        byte position,
        ref UInt128 seenThrows,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 friends = board.BitboardForFriendOf(piece.Color);
        bool isWhite = piece.Color is BitPieceColor.White;
        BitPieceColor enemyColor = isWhite ? BitPieceColor.Black : BitPieceColor.White;
        UInt128 enemies = board.BitboardForEnemyOf(piece.Color);
        UInt128 nonPawnEnemies =
            enemies
            & ~board.BitboardFor(PieceType.Pawn, enemyColor)
            & ~board.BitboardFor(PieceType.UnderagePawn, enemyColor);

        UInt128 positionBit = UInt128.One << position;
        (UInt128 throwMask, UInt128 attackableSquares) = isWhite
            ? GenerateWhiteThrowMask(board, positionBit, position, nonPawnEnemies)
            : GenerateBlackThrowMask(board, positionBit, position, nonPawnEnemies);

        throwMask &= ~friends;
        throwMask &= ~seenThrows;
        seenThrows |= throwMask;

        UInt128 stuns = throwMask & nonPawnEnemies;
        throwMask &= ~enemies;
        throwMask &= attackableSquares;

        while (throwMask != 0)
        {
            byte throwTo = BitboardHelpers.BitScanForward(ref throwMask);
            moves[moveCount++] = new BitMove()
            {
                From = position,
                To = throwTo,
                Piece = piece,
                SpecialMoveType = SpecialMoveType.Throw,
            };
        }

        while (stuns != 0)
        {
            byte stunAt = BitboardHelpers.BitScanForward(ref stuns);
            moves[moveCount++] = new BitMove()
            {
                From = position,
                To = stunAt,
                Piece = piece,
                SpecialMoveType = SpecialMoveType.Throw,
                CapturesMask = positionBit,
            };
        }
    }

    private static (UInt128 throwMask, UInt128 attackableSquares) GenerateWhiteThrowMask(
        BitBoard board,
        UInt128 positionBit,
        byte position,
        UInt128 nonPawnEnemies
    )
    {
        UInt128 mask = 0;

        UInt128 forwardThrower = BitboardHelpers.ShiftDown(positionBit);
        if (
            (board.ValidWhiteThrowers & forwardThrower) != 0
            && (board.StunnedPieces & forwardThrower) == 0
        )
        {
            mask |= PieceMasks.WhiteThrowForwardMasks[position];
        }

        UInt128 rightThrower = BitboardHelpers.ShiftDownLeft(positionBit);
        if (
            (board.ValidWhiteThrowers & rightThrower) != 0
            && (board.StunnedPieces & rightThrower) == 0
        )
        {
            mask |= PieceMasks.WhiteThrowRightMasks[position];
        }

        UInt128 leftThrower = BitboardHelpers.ShiftDownRight(positionBit);
        if (
            (board.ValidWhiteThrowers & leftThrower) != 0
            && (board.StunnedPieces & leftThrower) == 0
        )
        {
            mask |= PieceMasks.WhiteThrowLeftMasks[position];
        }

        return (
            throwMask: mask,
            attackableSquares: BitboardHelpers.ShiftDownRight(nonPawnEnemies)
                | BitboardHelpers.ShiftDownLeft(nonPawnEnemies)
        );
    }

    private static (UInt128 throwMask, UInt128 attackableSquares) GenerateBlackThrowMask(
        BitBoard board,
        UInt128 positionBit,
        byte position,
        UInt128 nonPawnEnemies
    )
    {
        UInt128 mask = 0;

        UInt128 forwardThrower = BitboardHelpers.ShiftUp(positionBit);
        if (
            (board.ValidBlackThrowers & forwardThrower) != 0
            && (board.StunnedPieces & forwardThrower) == 0
        )
        {
            mask |= PieceMasks.BlackThrowForwardMasks[position];
        }

        UInt128 rightThrower = BitboardHelpers.ShiftUpLeft(positionBit);
        if (
            (board.ValidBlackThrowers & rightThrower) != 0
            && (board.StunnedPieces & rightThrower) == 0
        )
        {
            mask |= PieceMasks.BlackThrowRightMasks[position];
        }

        UInt128 leftThrower = BitboardHelpers.ShiftUpRight(positionBit);
        if (
            (board.ValidBlackThrowers & leftThrower) != 0
            && (board.StunnedPieces & leftThrower) == 0
        )
        {
            mask |= PieceMasks.BlackThrowLeftMasks[position];
        }

        return (
            throwMask: mask,
            attackableSquares: BitboardHelpers.ShiftUpRight(nonPawnEnemies)
                | BitboardHelpers.ShiftUpLeft(nonPawnEnemies)
        );
    }
}
