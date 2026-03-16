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

    public const int BotTraitorRookValue = 250;

    public int SeeCapture(BitMove move, BitBoard board)
    {
        // traitor rooks can be lost even without a capture
        if (move.CapturesMask == 0 && move.Piece.Type is not PieceType.TraitorRook)
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

        MoveUndoState undo = board.MakeMove(move);
        int value = captureValue - See(move.To, move.Piece, board);
        board.UndoMove(undo);
        return value;
    }

    private int See(byte position, BitPiece capturedByPiece, BitBoard board)
    {
        UInt128 positionBit = UInt128.One << position;
        int capturedByPieceValue = GetRelativeValue(capturedByPiece, board);

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _bitMoveGenerator.Generate(board, moves, ref moveCount);

        UInt128 whiteOwnedTraitorRooks = FindOwnedTraitorRooks(board, isWhiteToMove: true);
        UInt128 blackOwnedTraitorRooks = FindOwnedTraitorRooks(board, isWhiteToMove: false);

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

        MoveUndoState undo = board.MakeMove(bestMove.Value);
        int value = Math.Max(
            0,
            totalCapturedValue
                - See(position: bestMove.Value.To, capturedByPiece: bestMove.Value.Piece, board)
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
            if (move.Piece.Color == capturedPiece.Value.Color)
            {
                captureValue -= value;
            }
            else
            {
                captureValue += value;
            }
        }

        UInt128 ourPrevTraitorRooks = board.IsWhiteToMove
            ? whiteOwnedTraitorRooks
            : blackOwnedTraitorRooks;
        UInt128 enemyPrevTraitorRooks = board.IsWhiteToMove
            ? blackOwnedTraitorRooks
            : whiteOwnedTraitorRooks;

        MoveUndoState undo = board.MakeMove(move);
        UInt128 ourNewTraitorRooks = FindOwnedTraitorRooks(
            board,
            isWhiteToMove: !board.IsWhiteToMove
        );
        UInt128 enemyNewTraitorRooks = FindOwnedTraitorRooks(
            board,
            isWhiteToMove: board.IsWhiteToMove
        );
        board.UndoMove(undo);

        int ourLostTraitorRooks = BitboardHelpers.CountBits(
            ourPrevTraitorRooks & ~ourNewTraitorRooks
        );
        int enemyLostTraitorRooks = BitboardHelpers.CountBits(
            enemyPrevTraitorRooks & ~enemyNewTraitorRooks
        );
        captureValue -= ourLostTraitorRooks * BotTraitorRookValue;
        captureValue += enemyLostTraitorRooks * BotTraitorRookValue;

        return captureValue;
    }

    private static int GetRelativeValue(BitPiece piece, BitBoard board)
    {
        if (piece.Type is PieceType.King && CountKings(piece.Color, board) <= 1)
        {
            return 100_000;
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
