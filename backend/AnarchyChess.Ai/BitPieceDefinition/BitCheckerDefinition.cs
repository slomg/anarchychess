using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public struct CheckerHop
{
    public byte FirstHopPosition;
    public byte SecondHopPosition;

    public UInt128 FirstHop;
    public UInt128 SecondHop;
}

public sealed class BitCheckerDefinition : IBitPieceDefinition
{
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
        UInt128 promotionMask =
            piece.Color is BitPieceColor.White
                ? BitboardConstants.TopEdgeMask
                : BitboardConstants.BottomEdgeMask;

        GenerateHops(
            board,
            origin: position,
            position: position,
            piece,
            captures: 0,
            visited: UInt128.One << position,
            promotionMask: promotionMask,
            isFirstHop: true,
            moves,
            ref moveCount
        );
    }

    private static void GenerateHops(
        BitBoard board,
        byte origin,
        byte position,
        BitPiece piece,
        UInt128 captures,
        UInt128 visited,
        UInt128 promotionMask,
        bool isFirstHop,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 positionBit = UInt128.One << position;

        RecurseToDirection(
            board,
            piece,
            origin: origin,
            captures: captures,
            visited: visited,
            promotionMask: promotionMask,
            hop: RecurseUpRight(position, positionBit),
            isFirstHop: isFirstHop,
            moves,
            ref moveCount
        );
        RecurseToDirection(
            board,
            piece,
            origin: origin,
            captures: captures,
            visited: visited,
            promotionMask: promotionMask,
            hop: RecurseUpLeft(position, positionBit),
            isFirstHop: isFirstHop,
            moves,
            ref moveCount
        );
        RecurseToDirection(
            board,
            piece,
            origin: origin,
            captures: captures,
            visited: visited,
            promotionMask: promotionMask,
            hop: RecurseDownRight(position, positionBit),
            isFirstHop: isFirstHop,
            moves,
            ref moveCount
        );
        RecurseToDirection(
            board,
            piece,
            origin: origin,
            captures: captures,
            visited: visited,
            promotionMask: promotionMask,
            hop: RecurseDownLeft(position, positionBit),
            isFirstHop: isFirstHop,
            moves,
            ref moveCount
        );
    }

    private static CheckerHop RecurseUpRight(byte position, UInt128 positionBit)
    {
        byte firstHopPosition = (byte)(position + 11);
        byte secondHopPosition = (byte)(position + 22);

        UInt128 firstHop = (positionBit & BitboardConstants.TopRightEdgeExcludeMask) << 11;
        UInt128 secondHop = (firstHop & BitboardConstants.TopRightEdgeExcludeMask) << 11;
        return new()
        {
            FirstHop = firstHop,
            FirstHopPosition = firstHopPosition,

            SecondHop = secondHop,
            SecondHopPosition = secondHopPosition,
        };
    }

    private static CheckerHop RecurseUpLeft(byte position, UInt128 positionBit)
    {
        byte firstHopPosition = (byte)(position + 9);
        byte secondHopPosition = (byte)(position + 18);

        UInt128 firstHop = (positionBit & BitboardConstants.TopLeftEdgeExcludeMask) << 9;
        UInt128 secondHop = (firstHop & BitboardConstants.TopLeftEdgeExcludeMask) << 9;
        return new()
        {
            FirstHop = firstHop,
            FirstHopPosition = firstHopPosition,

            SecondHop = secondHop,
            SecondHopPosition = secondHopPosition,
        };
    }

    private static CheckerHop RecurseDownRight(byte position, UInt128 positionBit)
    {
        byte firstHopPosition = (byte)(position - 9);
        byte secondHopPosition = (byte)(position - 18);

        UInt128 firstHop = (positionBit & BitboardConstants.BottomRightEdgeExcludeMask) >> 9;
        UInt128 secondHop = (firstHop & BitboardConstants.BottomRightEdgeExcludeMask) >> 9;
        return new()
        {
            FirstHop = firstHop,
            FirstHopPosition = firstHopPosition,

            SecondHop = secondHop,
            SecondHopPosition = secondHopPosition,
        };
    }

    private static CheckerHop RecurseDownLeft(byte position, UInt128 positionBit)
    {
        byte firstHopPosition = (byte)(position - 11);
        byte secondHopPosition = (byte)(position - 22);

        UInt128 firstHop = (positionBit & BitboardConstants.BottomLeftEdgeExcludeMask) >> 11;
        UInt128 secondHop = (firstHop & BitboardConstants.BottomLeftEdgeExcludeMask) >> 11;
        return new()
        {
            FirstHop = firstHop,
            FirstHopPosition = firstHopPosition,

            SecondHop = secondHop,
            SecondHopPosition = secondHopPosition,
        };
    }

    private static void RecurseToDirection(
        BitBoard board,
        BitPiece piece,
        byte origin,
        UInt128 captures,
        UInt128 visited,
        UInt128 promotionMask,
        CheckerHop hop,
        bool isFirstHop,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        if (hop.FirstHop == 0)
        {
            return;
        }

        bool isFirstHopEmpty = (hop.FirstHop & board.Occupancy) == 0;
        bool isSecondHopEmpty = (hop.SecondHop & board.Occupancy) == 0;
        if (isFirstHopEmpty && isFirstHop)
        {
            moves[moveCount++] = new BitMove()
            {
                From = origin,
                To = hop.FirstHopPosition,
                Piece = piece,
                CapturesMask = captures,
                PromotesTo = GetPromotionPiece(hop.FirstHop, promotionMask),
            };
        }

        if (hop.SecondHop == 0 || (visited & hop.SecondHop) != 0)
        {
            return;
        }

        if (isFirstHopEmpty && isSecondHopEmpty && isFirstHop)
        {
            moves[moveCount++] = new BitMove()
            {
                From = origin,
                To = hop.SecondHopPosition,
                Piece = piece,
                CapturesMask = captures,
                PromotesTo = GetPromotionPiece(hop.SecondHop, promotionMask),
            };
        }

        if (!isFirstHopEmpty && isSecondHopEmpty)
        {
            captures |= hop.FirstHop & board.BitboardForEnemyOf(piece.Color);

            moves[moveCount++] = new BitMove()
            {
                From = origin,
                To = hop.SecondHopPosition,
                Piece = piece,
                CapturesMask = captures,
                PromotesTo = GetPromotionPiece(hop.SecondHop, promotionMask),
            };

            visited |= hop.SecondHop;

            GenerateHops(
                board,
                origin: origin,
                position: hop.SecondHopPosition,
                piece,
                captures: captures,
                visited: visited,
                promotionMask: promotionMask,
                isFirstHop: false,
                moves,
                ref moveCount
            );
        }
    }

    private static PieceType? GetPromotionPiece(UInt128 positionBit, UInt128 promotionMask) =>
        (positionBit & promotionMask) != 0 ? PieceType.King : null;
}
