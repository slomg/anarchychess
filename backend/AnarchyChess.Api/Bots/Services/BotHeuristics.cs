using AnarchyChess.Ai;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotHeuristics
{
    bool CausesForcedMove(BitMove move, BotHeuristicContext context);
    bool IsBackwards(BitMove move);
    bool IsCapturingOpponentHang(BitMove move, BotHeuristicContext context);
    bool IsEdge(BitMove move);
    bool IsHang(BitMove move, BotHeuristicContext context);
    bool IsMultiStep(BitMove move, BotHeuristicContext context);
    bool IsNonCentralPawn(BitMove move);
    bool IsRecapture(BitMove move, BotHeuristicContext context);
    bool IsSameAsPieceAsLast(BitMove move, BotHeuristicContext context);
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

public class BotHeuristics(IBotSee botSee) : IBotHeuristics
{
    private readonly IBotSee _botSee = botSee;

    public bool IsSameAsPieceAsLast(BitMove move, BotHeuristicContext context)
    {
        if (context.Board.Moves.Count == 0)
        {
            return false;
        }

        Move lastMove = context.Board.Moves[^1];
        return move.From == lastMove.To.AsIdx();
    }

    public bool IsNonCentralPawn(BitMove move)
    {
        if (move.Piece.Type is not PieceType.Pawn)
        {
            return false;
        }

        const int centralMin = 3;
        const int centralMax = 6;
        int x = move.From % 10;
        return x < centralMin || x > centralMax;
    }

    public bool IsEdge(BitMove move)
    {
        int toX = move.To % 10;
        return toX == 0 || toX == 9;
    }

    public bool IsRecapture(BitMove move, BotHeuristicContext context)
    {
        if (context.Board.Moves.Count == 0)
        {
            return false;
        }

        Move lastMove = context.Board.Moves[^1];
        if (lastMove.Captures.Count == 0)
        {
            return false;
        }

        UInt128 lastMoveTo = UInt128.One << lastMove.To.AsIdx();
        return (move.CapturesMask & lastMoveTo) != 0;
    }

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
        _botSee.CheckMultiStep(move, context.Bitboard);

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
            BitMoveGenerator.Generate(
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
        int exchangeValue = _botSee.SeeCapture(move, context.Bitboard);
        if (exchangeValue < 0)
        {
            return true;
        }

        UInt128 positionBit = UInt128.One << move.To;
        for (int i = 0; i < context.OpponentMoveCount; i++)
        {
            BitMove opponentMove = context.OpponentMoves[i];
            // we know this is either an equal or winning exchance
            if (
                exchangeValue >= 0
                && move.CapturesMask != 0
                && (opponentMove.CapturesMask & positionBit) != 0
            )
            {
                continue;
            }

            if (_botSee.SeeCapture(opponentMove, context.BitboardAfterMove) > exchangeValue)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCapturingOpponentHang(BitMove move, BotHeuristicContext context) =>
        _botSee.SeeCapture(move, context.Bitboard) > 90;
}
