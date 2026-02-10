using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;

namespace AnarchyChess.Ai;

public partial class BitBoard
{
    public UInt128[,] Bitboards { get; }
    public BitPiece?[] PieceAt { get; }

    public UInt128 WhitePieces { get; private set; }
    public UInt128 BlackPieces { get; private set; }
    public UInt128 NeutralPieces { get; private set; }
    public UInt128 Occupancy { get; private set; }
    public UInt128 Empty { get; private set; }

    public UInt128 HasMoved { get; private set; }

    public UInt128 WhiteEnemy { get; private set; }
    public UInt128 BlackEnemy { get; private set; }

    public bool IsWhiteToMove { get; private set; }

    public UInt128 EnPassantSquaresMask { get; private set; }
    public byte EnPassantPawnSquare { get; private set; }

    public BitBoard(
        UInt128[,]? bitboards = null,
        UInt128? hasMoved = null,
        BitPiece?[]? pieceAt = null,
        bool isWhiteToMove = true,
        BitMove? prevMove = null
    )
    {
        Bitboards =
            bitboards
            ?? new UInt128[
                Enum.GetValues<BitPieceColor>().Length,
                Enum.GetValues<PieceType>().Length
            ];
        PieceAt = pieceAt ?? new BitPiece?[10 * 10];
        HasMoved = hasMoved ?? 0;
        IsWhiteToMove = isWhiteToMove;

        for (int i = 0; i < Enum.GetValues<PieceType>().Length; i++)
        {
            WhitePieces |= Bitboards[(int)BitPieceColor.White, i];
            BlackPieces |= Bitboards[(int)BitPieceColor.Black, i];
            NeutralPieces |= Bitboards[(int)BitPieceColor.Neutral, i];
        }

        ComputeAggregateBitboards();
        if (prevMove is not null)
        {
            ProcessMoveEffects(prevMove.Value);
        }
    }

    public static BitBoard FromPieces(
        Dictionary<AlgebraicPoint, Piece> pieces,
        bool isWhiteToMove = true,
        BitMove? prevMove = null
    )
    {
        UInt128[,] bitboards = new UInt128[
            Enum.GetValues<BitPieceColor>().Length,
            Enum.GetValues<PieceType>().Length
        ];
        BitPiece?[] pieceAt = new BitPiece?[10 * 10];

        UInt128 hasMoved = 0;

        foreach (var (point, piece) in pieces)
        {
            BitPieceColor color = piece.Color.Match(
                whenWhite: BitPieceColor.White,
                whenBlack: BitPieceColor.Black,
                whenNeutral: BitPieceColor.Neutral
            );
            byte idx = point.AsIdx();
            bitboards[(int)color, (int)piece.Type] |= UInt128.One << idx;
            pieceAt[idx] = new BitPiece() { Type = piece.Type, Color = color };

            if (piece.HasMoved)
            {
                hasMoved |= UInt128.One << idx;
            }
        }

        return new BitBoard(
            bitboards,
            hasMoved,
            pieceAt,
            isWhiteToMove: isWhiteToMove,
            prevMove: prevMove
        );
    }

    public ref UInt128 BitboardFor(PieceType pieceType, BitPieceColor color) =>
        ref Bitboards[(int)color, (int)pieceType];

    public bool HasPieceMoved(byte position) => (HasMoved & (UInt128.One << position)) != 0;

    public bool HasPieceMoved(UInt128 mask) => (HasMoved & mask) != 0;

    public UInt128 BitboardForFriendOf(BitPieceColor color) =>
        color switch
        {
            BitPieceColor.White => WhitePieces,
            BitPieceColor.Black => BlackPieces,
            _ => 0,
        };

    public UInt128 BitboardForEnemyOf(BitPieceColor color) =>
        color switch
        {
            BitPieceColor.White => WhiteEnemy,
            BitPieceColor.Black => BlackEnemy,
            _ => 0,
        };

    public bool TryGetPieceAt(byte position, [NotNullWhen(true)] out BitPiece? piece)
    {
        piece = PieceAt[position];
        return piece is not null;
    }

    public BitPiece? GetPieceAt(byte position) => PieceAt[position];

    public MoveUndoState MakeMove(BitMove move)
    {
        MoveUndoState undoState = new()
        {
            From = move.From,
            To = move.To,
            Piece = move.Piece,
            PromotedTo = move.PromotesTo,
            SpecialMoveType = move.SpecialMoveType,

            PrevHasMoved = HasMoved,

            PrevEnPassantSquaresMask = EnPassantSquaresMask,
            PrevEnPassantPawnSquare = EnPassantPawnSquare,

            PrevIsWhiteToMove = IsWhiteToMove,
        };

        UInt128 captureMask = move.CapturesMask;
        while (captureMask != 0)
        {
            byte captureSquare = (byte)BitboardHelpers.BitScanForward(ref captureMask);
            if (TryGetPieceAt(captureSquare, out var piece))
            {
                RemovePiece(piece.Value.Type, piece.Value.Color, captureSquare);
                undoState.AddCapture(captureSquare, piece.Value.Type, piece.Value.Color);
            }
        }

        ref UInt128 movingBitboard = ref BitboardFor(move.Piece.Type, move.Piece.Color);
        MovePiece(
            ref movingBitboard,
            move.From,
            move.To,
            move.Piece.Color,
            promotesTo: move.PromotesTo
        );

        ApplySpecialMove(move);
        ComputeAggregateBitboards();
        ProcessMoveEffects(move);
        IsWhiteToMove = !IsWhiteToMove;

        return undoState;
    }

    public void UndoMove(MoveUndoState undoState)
    {
        ref UInt128 movingBitboard = ref BitboardFor(
            undoState.PromotedTo ?? undoState.Piece.Type,
            undoState.Piece.Color
        );
        MovePiece(
            ref movingBitboard,
            from: undoState.To,
            to: undoState.From,
            undoState.Piece.Color,
            promotesTo: undoState.Piece.Type
        );

        for (int i = 0; i < undoState.CaptureCount; i++)
        {
            (byte position, PieceType pieceType, BitPieceColor color) = undoState.GetCapture(i);
            SpawnPiece(pieceType, color, position);
        }
        UndoSpecialMove(undoState);

        IsWhiteToMove = undoState.PrevIsWhiteToMove;
        HasMoved = undoState.PrevHasMoved;
        EnPassantSquaresMask = undoState.PrevEnPassantSquaresMask;
        EnPassantPawnSquare = undoState.PrevEnPassantPawnSquare;
        ComputeAggregateBitboards();
    }

    private void MovePiece(
        ref UInt128 bitboard,
        byte from,
        byte to,
        BitPieceColor color,
        PieceType? promotesTo = null
    )
    {
        UInt128 fromMask = UInt128.One << from;
        UInt128 toMask = UInt128.One << to;

        bitboard &= ~fromMask;
        HasMoved &= ~fromMask;

        if (promotesTo is PieceType promoteToPiece)
        {
            PromotePiece(position: from, toMask, promoteToPiece, color);
        }
        else
        {
            bitboard |= toMask;
            HasMoved |= toMask;
        }

        switch (color)
        {
            case BitPieceColor.White:
                WhitePieces = (WhitePieces & ~fromMask) | toMask;
                break;
            case BitPieceColor.Black:
                BlackPieces = (BlackPieces & ~fromMask) | toMask;
                break;
            case BitPieceColor.Neutral:
                NeutralPieces = (NeutralPieces & ~fromMask) | toMask;
                break;
        }
        (PieceAt[from], PieceAt[to]) = (null, PieceAt[from]);
    }

    private void PromotePiece(
        byte position,
        UInt128 toMask,
        PieceType promoteToPiece,
        BitPieceColor color
    )
    {
        ref UInt128 promotionBitboard = ref BitboardFor(promoteToPiece, color);
        promotionBitboard |= toMask;
        HasMoved &= ~toMask;

        if (TryGetPieceAt(position, out var promotedPiece))
        {
            var updatedPiece = promotedPiece.Value;
            updatedPiece.Type = promoteToPiece;
            PieceAt[position] = updatedPiece;
        }
    }

    private void SpawnPiece(PieceType pieceType, BitPieceColor color, byte at)
    {
        if (TryGetPieceAt(at, out var piece))
        {
            RemovePiece(piece.Value.Type, piece.Value.Color, at);
        }

        UInt128 mask = UInt128.One << at;
        ref UInt128 bitboard = ref BitboardFor(pieceType, color);
        bitboard |= mask;

        switch (color)
        {
            case BitPieceColor.White:
                WhitePieces |= mask;
                break;
            case BitPieceColor.Black:
                BlackPieces |= mask;
                break;
            case BitPieceColor.Neutral:
                NeutralPieces |= mask;
                break;
        }
        PieceAt[at] = new BitPiece() { Type = pieceType, Color = color };
    }

    private void RemovePiece(PieceType pieceType, BitPieceColor color, byte at)
    {
        UInt128 inverseMask = ~(UInt128.One << at);
        ref UInt128 bitboard = ref BitboardFor(pieceType, color);
        bitboard &= inverseMask;

        switch (color)
        {
            case BitPieceColor.White:
                WhitePieces &= inverseMask;
                break;
            case BitPieceColor.Black:
                BlackPieces &= inverseMask;
                break;
            case BitPieceColor.Neutral:
                NeutralPieces &= inverseMask;
                break;
        }
        PieceAt[at] = null;
        HasMoved &= inverseMask;
    }

    private void ComputeAggregateBitboards()
    {
        Occupancy = WhitePieces | BlackPieces | NeutralPieces;
        Empty = ~Occupancy;

        WhiteEnemy = BlackPieces | NeutralPieces;
        BlackEnemy = WhitePieces | NeutralPieces;
    }

    private void ProcessMoveEffects(BitMove move)
    {
        if (GameLogicConstants.PawnLikePieces.Contains(move.Piece.Type) && move.From != move.To)
        {
            int fromRank = move.From / 10;
            int toRank = move.To / 10;
            int file = move.From % 10;

            int step = (toRank > fromRank) ? 1 : -1;
            for (int rank = fromRank + step; rank != toRank; rank += step)
            {
                EnPassantSquaresMask |= UInt128.One << (rank * 10 + file);
            }

            EnPassantPawnSquare = move.To;
        }
        else
        {
            EnPassantSquaresMask = 0;
            EnPassantPawnSquare = 0;
        }
    }
}
