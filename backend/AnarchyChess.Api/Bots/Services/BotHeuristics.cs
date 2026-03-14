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
    bool IsEdge(BitMove move);
    bool IsHang(BitMove move, BotHeuristicContext context);
    bool IsMultiStep(BitMove move, BotHeuristicContext context);
    bool IsNonCentralPawn(BitMove move);
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
        if (exchangeValue < 0)
        {
            return true;
        }
        bool isWinningExchange = exchangeValue > 0;

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
        ((UInt128.One << move.From) & move.CapturesMask) == 0
        && SeeCapture(move, context.Bitboard) > 90;

    private int SeeCapture(BitMove move, BitBoard board)
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

        whiteOwnedTraitorRooks &= ~move.CapturesMask;
        blackOwnedTraitorRooks &= ~move.CapturesMask;
        if (move.Piece.Type is PieceType.TraitorRook && move.Piece.Color is BitPieceColor.White)
        {
            whiteOwnedTraitorRooks &= ~(UInt128.One << move.From);
            whiteOwnedTraitorRooks |= UInt128.One << move.To;
        }
        else if (
            move.Piece.Type is PieceType.TraitorRook
            && move.Piece.Color is BitPieceColor.Black
        )
        {
            blackOwnedTraitorRooks &= ~(UInt128.One << move.From);
            blackOwnedTraitorRooks |= UInt128.One << move.To;
        }

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

        whiteOwnedTraitorRooks &= ~bestMove.Value.CapturesMask;
        blackOwnedTraitorRooks &= ~bestMove.Value.CapturesMask;
        if (
            bestMove.Value.Piece.Type is PieceType.TraitorRook
            && bestMove.Value.Piece.Color is BitPieceColor.White
        )
        {
            whiteOwnedTraitorRooks &= ~(UInt128.One << bestMove.Value.From);
            whiteOwnedTraitorRooks |= UInt128.One << bestMove.Value.To;
        }
        else if (
            bestMove.Value.Piece.Type is PieceType.TraitorRook
            && bestMove.Value.Piece.Color is BitPieceColor.Black
        )
        {
            blackOwnedTraitorRooks &= ~(UInt128.One << bestMove.Value.From);
            blackOwnedTraitorRooks |= UInt128.One << bestMove.Value.To;
        }

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
            }
            else if (blackAdjacentCount > whiteAdjacentCount && !isWhiteToMove)
            {
                mask |= UInt128.One << position;
            }
        }

        return mask;
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

                UInt128 checkerHops = PieceMasks.SingleCheckerJumpMasks[move.From];
                UInt128 checkerCaptures = PieceMasks.AdjacentMasks[move.From];
                UInt128 checkerAttacks = checkerHops & checkerCaptures;

                // make sure all captures would be possible in a single hop
                // and that all hops are possible in a single hop
                return (move.CapturesMask & checkerCaptures) != move.CapturesMask
                    || (checkerAttacks & (UInt128.One << move.To)) == 0;
            default:
                return false;
        }
    }
}
