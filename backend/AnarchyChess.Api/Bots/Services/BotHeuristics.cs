using AnarchyChess.Ai;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotHeuristics
{
    bool CausesForcedMove(BitMove move, BotHeuristicContext context);
    bool IsBackwards(BitMove move);
    bool IsCapturingOpponentHang(BitMove move, BotHeuristicContext context);
    bool IsHang(BitMove move, BotHeuristicContext context);
    bool IsMultiStep(BitMove move, BotHeuristicContext context);
    bool IsRecapture(BitMove move, BotHeuristicContext context);
    bool LosesKingCastlingRight(BitMove move, BotHeuristicContext context);
    bool LosesRookCastlingRight(BitMove move, BotHeuristicContext context);
}

public record BotHeuristicContext(
    IReadOnlyChessBoard Board,
    BitBoard Bitboard,
    BitBoard BitboardAfterMove,
    BitMove[] OpponentMoves,
    int OpponentMoveCount
);

public class BotHeuristics(IBitMoveGenerator bitMoveGenerator) : IBotHeuristics
{
    private readonly IBitMoveGenerator _bitMoveGenerator = bitMoveGenerator;

    public bool IsRecapture(BitMove move, BotHeuristicContext context) =>
        context.Board.Moves.Count > 0
        && context.Board.Moves[^1].Captures.Count > 0
        && move.To == context.Board.Moves[^1].To.AsIdx();

    public bool IsBackwards(BitMove move)
    {
        if (move.Piece.Color is BitPieceColor.Neutral)
        {
            return false;
        }

        int backwardsDirection = move.Piece.Color is BitPieceColor.White ? 1 : -1;
        int fromY = move.From / 10;
        int toY = move.To / 10;
        return (fromY - toY) * backwardsDirection > 0;
    }

    public bool IsMultiStep(BitMove move, BotHeuristicContext context) =>
        CheckMultiStep(move, context.Bitboard);

    public bool LosesKingCastlingRight(BitMove move, BotHeuristicContext context)
    {
        if (move.Piece.Type is not PieceType.King)
        {
            return false;
        }

        if (context.Bitboard.HasPieceMoved(move.From))
        {
            return false;
        }

        return true;
    }

    public bool LosesRookCastlingRight(BitMove move, BotHeuristicContext context)
    {
        if (move.Piece.Type is not PieceType.Rook)
        {
            return false;
        }

        if (context.Bitboard.HasPieceMoved(move.From))
        {
            return false;
        }

        return true;
    }

    public bool CausesForcedMove(BitMove move, BotHeuristicContext context)
    {
        if (context.OpponentMoves[0].ForcedMovePriority is not ForcedMovePriority.None)
        {
            return true;
        }

        if (move.CapturesMask == 0)
        {
            return false;
        }

        UInt128 positionBit = UInt128.One << move.To;
        Span<BitMove> opponentResponseMoves = stackalloc BitMove[EngineConstants.MaxMoves];
        for (int i = 0; i < context.OpponentMoveCount; i++)
        {
            BitMove opponentMove = context.OpponentMoves[i];
            if ((opponentMove.CapturesMask & positionBit) == 0)
            {
                continue;
            }

            MoveUndoState undo = context.BitboardAfterMove.MakeMove(opponentMove);
            NullMoveUndoState nullUndo = context.BitboardAfterMove.MakeNullMove();

            int opponentResponseMoveCount = 0;
            _bitMoveGenerator.Generate(
                context.BitboardAfterMove,
                opponentResponseMoves,
                ref opponentResponseMoveCount
            );

            context.BitboardAfterMove.UndoNullMove(nullUndo);
            context.BitboardAfterMove.UndoMove(undo);

            if (opponentResponseMoves[0].ForcedMovePriority is not ForcedMovePriority.None)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHang(BitMove move, BotHeuristicContext context)
    {
        int exchangeValue = SeeCapture(move, context.Bitboard);
        bool isWinningExchange = exchangeValue > 90;
        exchangeValue = Math.Max(90, exchangeValue);

        UInt128 positionBit = UInt128.One << move.To;
        for (int i = 0; i < context.OpponentMoveCount; i++)
        {
            BitMove opponentMove = context.OpponentMoves[i];
            //we already know this exchance is winning, so we don't need to check it again
            if (isWinningExchange && (opponentMove.CapturesMask & positionBit) != 0)
            {
                continue;
            }

            if (SeeCapture(opponentMove, context.BitboardAfterMove) > exchangeValue)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCapturingOpponentHang(BitMove move, BotHeuristicContext context) =>
        ((UInt128.One << move.To) & move.CapturesMask) == 0
        && SeeCapture(move, context.Bitboard) > 90;

    private int SeeCapture(BitMove move, BitBoard board)
    {
        UInt128 capturesMask = move.CapturesMask;
        if (capturesMask == 0)
        {
            return 0;
        }

        byte capturePosition = (byte)BitboardHelpers.BitScanForward(ref capturesMask);
        if (!board.TryGetPieceAt(capturePosition, out var capturedPiece))
        {
            return 0;
        }

        int capturedPieceValue = MaterialValue.GetPieceValue(capturedPiece.Value.Type);
        if (capturedPieceValue < 300)
        {
            return 0;
        }

        MoveUndoState undo = board.MakeMove(move);
        int value = capturedPieceValue - See(move.To, move.Piece.Type, board);
        board.UndoMove(undo);
        return value;
    }

    private int See(byte position, PieceType capturedByPiece, BitBoard board)
    {
        UInt128 positionBit = UInt128.One << position;
        BitPieceColor sideToMove = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;
        BitPieceColor opponentColor = board.IsWhiteToMove
            ? BitPieceColor.Black
            : BitPieceColor.White;

        int kingCount = CountKings(sideToMove, board);
        int capturedByPieceValue =
            kingCount <= 1 && capturedByPiece is PieceType.King
                ? 100_000
                : MaterialValue.GetPieceValue(capturedByPiece);

        int opponentKingValue =
            CountKings(opponentColor, board) <= 1
                ? 100_000
                : MaterialValue.GetPieceValue(PieceType.King);

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _bitMoveGenerator.Generate(board, moves, ref moveCount);

        BitMove? lowestAttack = null;
        int lowestAttackerValue = int.MaxValue;
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

            int attackerValue =
                move.Piece.Type is PieceType.King
                    ? opponentKingValue
                    : MaterialValue.GetPieceValue(move.Piece.Type);
            if (attackerValue < lowestAttackerValue)
            {
                lowestAttackerValue = attackerValue;
                lowestAttack = move;
            }
        }

        if (lowestAttack is null)
        {
            return 0;
        }

        MoveUndoState undo = board.MakeMove(lowestAttack.Value);
        int value = Math.Max(
            0,
            capturedByPieceValue
                - See(
                    position: lowestAttack.Value.To,
                    capturedByPiece: lowestAttack.Value.Piece.Type,
                    board
                )
        );
        board.UndoMove(undo);
        return value;
    }

    private static int CountKings(BitPieceColor color, BitBoard board) =>
        BitboardHelpers.CountBits(board.BitboardFor(PieceType.King, color));

    private static bool CheckMultiStep(BitMove move, BitBoard board)
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

                UInt128 checkerAttacks =
                    PieceMasks.SingleCheckerJumpMasks[move.From]
                    | PieceMasks.AdjacentMasks[move.From];
                return (checkerAttacks & (UInt128.One << move.To)) == 0;
            default:
                return false;
        }
    }
}
