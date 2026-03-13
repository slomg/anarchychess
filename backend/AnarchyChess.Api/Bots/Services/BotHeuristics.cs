using AnarchyChess.Ai;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotHeuristics
{
    bool CausesForcedMove(BitMove move, BitBoard boardAfterMove);
    bool IsBackwards(BitMove move);
    bool IsCapturingOpponentHang(BitMove move, BitBoard boardAfterMove);
    bool IsHang(BitMove move, BitBoard boardAfterMove);
    bool IsMultiStep(BitMove move, BitBoard boardBeforeMove);
    bool LosesKingCastlingRight(BitMove move, BitBoard board);
    bool LosesRookCastlingRight(BitMove move, BitBoard board);
}

public class BotHeuristics(IBitMoveGenerator bitMoveGenerator) : IBotHeuristics
{
    private readonly IBitMoveGenerator _bitMoveGenerator = bitMoveGenerator;

    private readonly PieceType[] _piecesByValue =
    [
        .. Enum.GetValues<PieceType>()
            .OrderBy(MaterialValue.GetPieceValue)
            .Where(x => MaterialValue.GetPieceValue(x) > 0),
    ];

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

    public bool IsMultiStep(BitMove move, BitBoard boardBeforeMove)
    {
        switch (move.Piece.Type)
        {
            case PieceType.Bishop:
                UInt128 bishopAttacks = MagicLibrary.GetAttacks(
                    MagicLibrary.BishopTable,
                    move.From,
                    boardBeforeMove.Occupancy
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

    public bool LosesKingCastlingRight(BitMove move, BitBoard board)
    {
        if (move.Piece.Type is not PieceType.King)
        {
            return false;
        }

        if (board.HasPieceMoved(move.From))
        {
            return false;
        }

        return true;
    }

    public bool LosesRookCastlingRight(BitMove move, BitBoard board)
    {
        if (move.Piece.Type is not PieceType.Rook)
        {
            return false;
        }

        if (board.HasPieceMoved(move.From))
        {
            return false;
        }

        return true;
    }

    public bool CausesForcedMove(BitMove move, BitBoard boardAfterMove)
    {
        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _bitMoveGenerator.Generate(boardAfterMove, moves, ref moveCount);
        if (moveCount == 0)
        {
            return false;
        }

        return moves[0].ForcedMovePriority is not ForcedMovePriority.None;
    }

    public bool IsHang(BitMove move, BitBoard boardAfterMove)
    {
        BitPieceColor ourSide = boardAfterMove.IsWhiteToMove
            ? BitPieceColor.Black
            : BitPieceColor.White;
        UInt128 ourPieces = boardAfterMove.BitboardForFriendOf(ourSide);

        Span<BitMove> opponentMoves = stackalloc BitMove[EngineConstants.MaxMoves];
        int moveCount = 0;
        _bitMoveGenerator.Generate(boardAfterMove, opponentMoves, ref moveCount);

        while (ourPieces != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref ourPieces);
            if (
                !boardAfterMove.TryGetPieceAt(position, out var piece)
                || MaterialValue.GetPieceValue(piece.Value.Type) < 300
            )
            {
                continue;
            }

            UInt128 positionBit = UInt128.One << position;
            for (int i = 0; i < moveCount; i++)
            {
                BitMove opponentMove = opponentMoves[i];
                if ((opponentMove.CapturesMask & positionBit) == 0)
                {
                    continue;
                }

                MoveUndoState undo = boardAfterMove.MakeMove(opponentMove);
                int seeValue = See(position, capturingPiece: piece.Value.Type, boardAfterMove);
                boardAfterMove.UndoMove(undo);
                if (seeValue < 90)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsCapturingOpponentHang(BitMove move, BitBoard boardAfterMove)
    {
        if (move.CapturesMask == 0)
        {
            return false;
        }

        UInt128 capturesMask = move.CapturesMask;
        while (capturesMask != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref capturesMask);
            if (
                !boardAfterMove.TryGetPieceAt(position, out var capturingPiece)
                || MaterialValue.GetPieceValue(capturingPiece.Value.Type) < 300
            )
            {
                continue;
            }

            int seeValue = See(position, capturingPiece.Value.Type, boardAfterMove);
            if (seeValue == 0)
            {
                return true;
            }
        }

        return false;
    }

    private int See(byte position, PieceType capturingPiece, BitBoard board)
    {
        UInt128 positionBit = UInt128.One << position;
        BitPieceColor sideToMove = board.IsWhiteToMove ? BitPieceColor.White : BitPieceColor.Black;
        int kingCount = CountKings(sideToMove, board);
        if (kingCount <= 1 && capturingPiece is PieceType.King)
        {
            return 100_000;
        }

        int capturingValue = MaterialValue.GetPieceValue(capturingPiece);

        Span<BitMove> moves = stackalloc BitMove[EngineConstants.MaxMoves];
        foreach (var pieceType in _piecesByValue)
        {
            int pieceValue = MaterialValue.GetPieceValue(pieceType);

            UInt128 bitboard = board.BitboardFor(pieceType, sideToMove);
            while (bitboard != 0)
            {
                byte piecePosition = (byte)BitboardHelpers.BitScanForward(ref bitboard);

                int moveCount = 0;
                _bitMoveGenerator.GenerateForPiece(
                    board,
                    piecePosition,
                    new() { Type = pieceType, Color = sideToMove },
                    moves,
                    ref moveCount
                );

                for (int i = 0; i < moveCount; i++)
                {
                    BitMove move = moves[i];
                    if ((move.CapturesMask & positionBit) == 0)
                    {
                        continue;
                    }

                    if (IsMultiStep(move, board))
                    {
                        continue;
                    }

                    MoveUndoState undo = board.MakeMove(move);
                    int value = Math.Max(
                        0,
                        capturingValue - See(position: move.To, capturingPiece: pieceType, board)
                    );
                    board.UndoMove(undo);
                    return value;
                }
            }
        }
        return 0;
    }

    private static int CountKings(BitPieceColor color, BitBoard board) =>
        BitboardHelpers.CountBits(board.BitboardFor(PieceType.King, color));
}
