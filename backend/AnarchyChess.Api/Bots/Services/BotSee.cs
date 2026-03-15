using AnarchyChess.Ai;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotSee
{
    bool CheckMultiStep(BitMove move, BitBoard board);
    int SeeCapture(BitMove move, BitBoard board);
}

public sealed class BotSee(IBitMoveGenerator bitMoveGenerator) : IBotSee
{
    private readonly IBitMoveGenerator _bitMoveGenerator = bitMoveGenerator;

    public int SeeCapture(BitMove move, BitBoard board)
    {
        if (move.CapturesMask == 0)
        {
            return 0;
        }

        UInt128 whiteOwnedTraitorRooks = FindOwnedTraitorRooks(board, isWhiteToMove: true);
        UInt128 blackOwnedTraitorRooks = FindOwnedTraitorRooks(board, isWhiteToMove: false);
        int captureValue = GetCaptureValue(
            move,
            board,
            whiteOwnedTraitorRooks,
            blackOwnedTraitorRooks
        );

        UpdateTraitorRookMasks(move, ref whiteOwnedTraitorRooks, ref blackOwnedTraitorRooks);

        MoveUndoState undo = board.MakeMove(move);
        int value =
            captureValue
            - See(move.To, move.Piece, board, whiteOwnedTraitorRooks, blackOwnedTraitorRooks);
        board.UndoMove(undo);
        return value;
    }

    private int See(
        byte position,
        BitPiece capturedByPiece,
        BitBoard board,
        UInt128 whiteOwnedTraitorRooks,
        UInt128 blackOwnedTraitorRooks
    )
    {
        UInt128 positionBit = UInt128.One << position;
        int capturedByPieceValue = GetRelativeValue(capturedByPiece, board);

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _bitMoveGenerator.Generate(board, moves, ref moveCount);

        BitMove? bestMove = null;
        int totalCapturedValue = 0;
        int bestNetGainScore = int.MinValue;
        for (int i = 0; i < moveCount; i++)
        {
            BitMove move = moves[i];
            if ((move.CapturesMask & positionBit) == 0)
            {
                continue;
            }

            if (CheckMultiStep(move, board))
            {
                continue;
            }

            int attackerValue = GetRelativeValue(move.Piece, board);
            int captureValue = GetCaptureValue(
                move,
                board,
                whiteOwnedTraitorRooks,
                blackOwnedTraitorRooks
            );
            int netGainScore = captureValue - attackerValue;
            if (netGainScore > bestNetGainScore)
            {
                bestMove = move;
                bestNetGainScore = netGainScore;
                totalCapturedValue = captureValue;
            }
        }
        if (bestMove is null)
        {
            return 0;
        }

        UpdateTraitorRookMasks(
            bestMove.Value,
            ref whiteOwnedTraitorRooks,
            ref blackOwnedTraitorRooks
        );

        MoveUndoState undo = board.MakeMove(bestMove.Value);
        int value = Math.Max(
            0,
            totalCapturedValue
                - See(
                    position: bestMove.Value.To,
                    capturedByPiece: bestMove.Value.Piece,
                    board,
                    whiteOwnedTraitorRooks,
                    blackOwnedTraitorRooks
                )
        );
        board.UndoMove(undo);
        return value;
    }

    private static int GetCaptureValue(
        BitMove move,
        BitBoard board,
        UInt128 whiteOwnedTraitorRooks,
        UInt128 blackOwnedTraitorRooks
    )
    {
        UInt128 ourTraitorRooks = board.IsWhiteToMove
            ? whiteOwnedTraitorRooks
            : blackOwnedTraitorRooks;

        UInt128 capturesMask = move.CapturesMask;
        int captureValue = 0;
        while (capturesMask != 0)
        {
            byte capturePosition = (byte)BitboardHelpers.BitScanForward(ref capturesMask);
            if (!board.TryGetPieceAt(capturePosition, out var capturedPiece))
            {
                continue;
            }

            int value = GetRelativeValue(capturedPiece.Value, board);

            bool ownsTraitorRook = (ourTraitorRooks & (UInt128.One << capturePosition)) != 0;
            if (capturedPiece.Value.Type is PieceType.TraitorRook && ownsTraitorRook)
            {
                captureValue -= value;
            }
            else if (capturedPiece.Value.Type is PieceType.TraitorRook && !ownsTraitorRook)
            {
                captureValue += value;
            }
            else if (move.Piece.Color == capturedPiece.Value.Color)
            {
                captureValue -= value;
            }
            else
            {
                captureValue += value;
            }
        }
        return captureValue;
    }

    private static int GetRelativeValue(BitPiece piece, BitBoard board)
    {
        if (piece.Type is PieceType.King && CountKings(piece.Color, board) <= 1)
        {
            return 100_000;
        }
        else if (piece.Type is PieceType.TraitorRook)
        {
            return 150;
        }
        else
        {
            return MaterialValue.GetPieceValue(piece.Type);
        }
    }

    private static UInt128 FindOwnedTraitorRooks(BitBoard board, bool isWhiteToMove)
    {
        UInt128 mask = 0;

        UInt128 traitorRooks = board.BitboardFor(PieceType.TraitorRook, BitPieceColor.Neutral);
        while (traitorRooks != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref traitorRooks);
            UInt128 adjacent = PieceMasks.AdjacentMasks[position];
            UInt128 whiteAdjacent = adjacent & board.WhitePieces;
            UInt128 blackAdjacent = adjacent & board.BlackPieces;

            int whiteAdjacentCount = BitboardHelpers.CountBits(whiteAdjacent);
            int blackAdjacentCount = BitboardHelpers.CountBits(blackAdjacent);
            if (whiteAdjacentCount > blackAdjacentCount && isWhiteToMove)
            {
                mask |= UInt128.One << position;
                continue;
            }
            else if (blackAdjacentCount > whiteAdjacentCount && !isWhiteToMove)
            {
                mask |= UInt128.One << position;
                continue;
            }

            if (position < 50 && isWhiteToMove)
            {
                mask |= UInt128.One << position;
            }
            else if (position > 50 && !isWhiteToMove)
            {
                mask |= UInt128.One << position;
            }
        }

        return mask;
    }

    private static void UpdateTraitorRookMasks(
        BitMove move,
        ref UInt128 whiteOwnedTraitorRooks,
        ref UInt128 blackOwnedTraitorRooks
    )
    {
        whiteOwnedTraitorRooks &= ~move.CapturesMask;
        blackOwnedTraitorRooks &= ~move.CapturesMask;

        UInt128 fromBit = UInt128.One << move.From;
        if (move.Piece.Type is PieceType.TraitorRook && (whiteOwnedTraitorRooks & fromBit) != 0)
        {
            whiteOwnedTraitorRooks &= ~(UInt128.One << move.From);
            whiteOwnedTraitorRooks |= UInt128.One << move.To;
        }
        else if (
            move.Piece.Type is PieceType.TraitorRook
            && (blackOwnedTraitorRooks & fromBit) != 0
        )
        {
            blackOwnedTraitorRooks &= ~(UInt128.One << move.From);
            blackOwnedTraitorRooks |= UInt128.One << move.To;
        }
    }

    private static int CountKings(BitPieceColor color, BitBoard board) =>
        BitboardHelpers.CountBits(board.BitboardFor(PieceType.King, color));

    public bool CheckMultiStep(BitMove move, BitBoard board)
    {
        switch (move.Piece.Type)
        {
            case PieceType.Bishop:
                UInt128 bishopAttacks = MagicLibrary.GetAttacks(
                    MagicLibrary.BishopTable,
                    move.From,
                    board.Occupancy
                );
                return (bishopAttacks & (UInt128.One << move.To)) == 0;
            case PieceType.Checker:
                if (BitboardHelpers.CountBits(move.CapturesMask) > 1)
                {
                    return true;
                }

                UInt128 checkerHops = PieceMasks.SingleCheckerJumpMasks[move.From];
                UInt128 checkerCaptures =
                    PieceMasks.AdjacentMasks[move.From]
                    & ~(board.BitboardForFriendOf(move.Piece.Color));
                UInt128 checkerAttacks = checkerHops | checkerCaptures;

                // make sure all captures would be possible in a single hop
                // and that all hops are possible in a single hop
                return (move.CapturesMask & checkerCaptures) != move.CapturesMask
                    || (checkerAttacks & (UInt128.One << move.To)) == 0;
            default:
                return false;
        }
    }
}
