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

    public UInt128 StunnedPieces { get; private set; }
    private readonly byte[] _stunnedForPlies;

    public UInt128 WhitePieces { get; private set; }
    public UInt128 BlackPieces { get; private set; }
    public UInt128 NeutralPieces { get; private set; }
    public UInt128 Occupancy { get; private set; }
    public UInt128 Empty { get; private set; }

    public UInt128 HasMoved { get; private set; }

    public UInt128 WhiteEnemy { get; private set; }
    public UInt128 BlackEnemy { get; private set; }

    public bool IsWhiteToMove { get; private set; } = true;

    public UInt128 EnPassantSquaresMask { get; private set; }
    public byte EnPassantPawnSquare { get; private set; }

    public UInt128 LastCaptureMask { get; private set; }

    public int WhiteMaterialCount { get; private set; }
    public int BlackMaterialCount { get; private set; }

    private BitBoard(
        UInt128[,] bitboards,
        BitPiece?[] pieceAt,
        UInt128 stunnedPieces,
        byte[] stunnedForPlies,
        PrevMoveState? prevMoveState
    )
    {
        Bitboards = bitboards;
        PieceAt = pieceAt;
        StunnedPieces = stunnedPieces;
        _stunnedForPlies = stunnedForPlies;

        for (int i = 0; i < Enum.GetValues<PieceType>().Length; i++)
        {
            WhitePieces |= Bitboards[(int)BitPieceColor.White, i];
            BlackPieces |= Bitboards[(int)BitPieceColor.Black, i];
            NeutralPieces |= Bitboards[(int)BitPieceColor.Neutral, i];
        }

        ComputeAggregateBitboards();

        if (prevMoveState is not null)
        {
            ProcessEnPassant(
                from: prevMoveState.From,
                to: prevMoveState.To,
                prevMoveState.SpecialMoveType,
                prevMoveState.Piece
            );
            LastCaptureMask = prevMoveState.CaptureMask;
        }
    }

    public BitBoard()
    {
        Bitboards = new UInt128[
            Enum.GetValues<BitPieceColor>().Length,
            Enum.GetValues<PieceType>().Length
        ];
        PieceAt = new BitPiece?[10 * 10];
        _stunnedForPlies = new byte[10 * 10];
    }

    public BitBoard(BitBoard other)
    {
        int colors = other.Bitboards.GetLength(0);
        int types = other.Bitboards.GetLength(1);
        Bitboards = new UInt128[colors, types];
        for (int color = 0; color < colors; color++)
        {
            for (int pieceType = 0; pieceType < types; pieceType++)
            {
                Bitboards[color, pieceType] = other.Bitboards[color, pieceType];
            }
        }

        PieceAt = new BitPiece?[other.PieceAt.Length];
        for (int i = 0; i < other.PieceAt.Length; i++)
        {
            BitPiece? otherPiece = other.PieceAt[i];
            PieceAt[i] = otherPiece is null
                ? null
                : new BitPiece() { Type = otherPiece.Value.Type, Color = otherPiece.Value.Color };
        }

        StunnedPieces = other.StunnedPieces;
        _stunnedForPlies = new byte[other._stunnedForPlies.Length];
        Array.Copy(other._stunnedForPlies, _stunnedForPlies, other._stunnedForPlies.Length);

        WhitePieces = other.WhitePieces;
        BlackPieces = other.BlackPieces;
        NeutralPieces = other.NeutralPieces;
        Occupancy = other.Occupancy;
        Empty = other.Empty;
        HasMoved = other.HasMoved;
        WhiteEnemy = other.WhiteEnemy;
        BlackEnemy = other.BlackEnemy;
        IsWhiteToMove = other.IsWhiteToMove;
        EnPassantSquaresMask = other.EnPassantSquaresMask;
        EnPassantPawnSquare = other.EnPassantPawnSquare;
        WhiteMaterialCount = other.WhiteMaterialCount;
        BlackMaterialCount = other.BlackMaterialCount;
        LastCaptureMask = other.LastCaptureMask;
        ValidBlackThrowers = other.ValidBlackThrowers;
        ValidWhiteThrowers = other.ValidWhiteThrowers;
    }

    public static BitBoard FromPieces(
        IReadOnlyDictionary<AlgebraicPoint, Piece> pieces,
        bool isWhiteToMove = true,
        IReadOnlyDictionary<AlgebraicPoint, int>? stunnedPositions = null,
        PrevMoveState? prevMoveState = null
    )
    {
        UInt128[,] bitboards = new UInt128[
            Enum.GetValues<BitPieceColor>().Length,
            Enum.GetValues<PieceType>().Length
        ];
        BitPiece?[] pieceAt = new BitPiece?[10 * 10];
        UInt128 hasMoved = 0;

        int whiteScore = 0;
        int blackScore = 0;

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

            if (piece.Color is GameColor.White)
            {
                whiteScore += MaterialValue.GetPieceValue(piece.Type);
            }
            else if (piece.Color is GameColor.Black)
            {
                blackScore += MaterialValue.GetPieceValue(piece.Type);
            }
        }

        byte[] stunnedForPlies = new byte[10 * 10];
        UInt128 stunnedPieces = 0;
        if (stunnedPositions is not null)
        {
            foreach (var (position, plyCount) in stunnedPositions)
            {
                stunnedPieces |= UInt128.One << position.AsIdx();
                stunnedForPlies[position.AsIdx()] = (byte)plyCount;
            }
        }

        return new BitBoard(
            bitboards,
            pieceAt,
            stunnedPieces: stunnedPieces,
            stunnedForPlies: stunnedForPlies,
            prevMoveState
        )
        {
            HasMoved = hasMoved,
            IsWhiteToMove = isWhiteToMove,
            WhiteMaterialCount = whiteScore,
            BlackMaterialCount = blackScore,
        };
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

    public NullMoveUndoState MakeNullMove()
    {
        NullMoveUndoState undo = new()
        {
            PrevIsWhiteToMove = IsWhiteToMove,
            PrevEnPassantPawnSquare = EnPassantPawnSquare,
            PrevEnPassantSquaresMask = EnPassantSquaresMask,
            PrevLastCaptureMask = LastCaptureMask,
        };
        IsWhiteToMove = !IsWhiteToMove;
        EnPassantPawnSquare = 0;
        EnPassantSquaresMask = 0;
        LastCaptureMask = 0;
        return undo;
    }

    public void UndoNullMove(NullMoveUndoState undo)
    {
        IsWhiteToMove = undo.PrevIsWhiteToMove;
        EnPassantPawnSquare = undo.PrevEnPassantPawnSquare;
        EnPassantSquaresMask = undo.PrevEnPassantSquaresMask;
        LastCaptureMask = undo.PrevLastCaptureMask;
    }

    public MoveUndoState MakeMove(BitMove move)
    {
        MoveUndoState undoState = new()
        {
            From = move.From,
            To = move.To,
            Piece = move.Piece,
            PromotedTo = move.PromotesTo,
            SpecialMoveType = move.SpecialMoveType,

            HasMoved = HasMoved,
            StunnedPieces = StunnedPieces,

            EnPassantSquaresMask = EnPassantSquaresMask,
            EnPassantPawnSquare = EnPassantPawnSquare,
            IsWhiteToMove = IsWhiteToMove,
            LastCaptureMask = LastCaptureMask,

            WhiteMaterialCount = WhiteMaterialCount,
            BlackMaterialCount = BlackMaterialCount,
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

        if (move.To != move.From || move.PromotesTo is not null)
        {
            MovePiece(
                move.Piece.Type,
                move.Piece.Color,
                move.From,
                move.To,
                promotesTo: move.PromotesTo
            );
        }

        ApplySpecialMove(move);
        ComputeAggregateBitboards();
        ProcessMoveEffects(move);
        DecrementStunned();
        IsWhiteToMove = !IsWhiteToMove;

        return undoState;
    }

    public void UndoMove(MoveUndoState undoState)
    {
        MovePiece(
            undoState.PromotedTo ?? undoState.Piece.Type,
            undoState.Piece.Color,
            from: undoState.To,
            to: undoState.From,
            promotesTo: undoState.Piece.Type
        );
        UndoSpecialMove(undoState);

        for (int i = 0; i < undoState.CaptureCount; i++)
        {
            (byte position, PieceType pieceType, BitPieceColor color) = undoState.GetCapture(i);
            SpawnPiece(pieceType, color, position);
        }

        UInt128 prevStunned = undoState.StunnedPieces;
        while (prevStunned != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref prevStunned);
            _stunnedForPlies[position]++;
        }

        WhiteMaterialCount = undoState.WhiteMaterialCount;
        BlackMaterialCount = undoState.BlackMaterialCount;

        IsWhiteToMove = undoState.IsWhiteToMove;
        HasMoved = undoState.HasMoved;
        StunnedPieces = undoState.StunnedPieces;
        EnPassantSquaresMask = undoState.EnPassantSquaresMask;
        EnPassantPawnSquare = undoState.EnPassantPawnSquare;
        LastCaptureMask = undoState.LastCaptureMask;

        ComputeAggregateBitboards();
    }

    private void MovePiece(
        PieceType pieceType,
        BitPieceColor color,
        byte from,
        byte to,
        PieceType? promotesTo = null
    )
    {
        if (promotesTo is PieceType promoteToPiece)
        {
            RemovePiece(pieceType, color, at: from);
            SpawnPiece(promoteToPiece, color, at: to);
            return;
        }

        ref UInt128 bitboard = ref BitboardFor(pieceType, color);

        UInt128 fromMask = UInt128.One << from;
        UInt128 toMask = UInt128.One << to;

        bitboard &= ~fromMask;
        HasMoved &= ~fromMask;
        bitboard |= toMask;
        HasMoved |= toMask;

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
                WhiteMaterialCount += MaterialValue.GetPieceValue(pieceType);
                break;
            case BitPieceColor.Black:
                BlackPieces |= mask;
                BlackMaterialCount += MaterialValue.GetPieceValue(pieceType);
                break;
            case BitPieceColor.Neutral:
                NeutralPieces |= mask;
                break;
        }
        PieceAt[at] = new BitPiece() { Type = pieceType, Color = color };
    }

    private void AddExistingPiece(PieceType pieceType, BitPieceColor color, byte at)
    {
        UInt128 mask = UInt128.One << at;
        ref UInt128 bitboard = ref BitboardFor(pieceType, color);
        bitboard |= mask;
        HasMoved |= mask;

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
                WhiteMaterialCount -= MaterialValue.GetPieceValue(pieceType);
                break;
            case BitPieceColor.Black:
                BlackPieces &= inverseMask;
                BlackMaterialCount -= MaterialValue.GetPieceValue(pieceType);
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
        ProcessEnPassant(move.From, move.To, move.SpecialMoveType, move.Piece);
        LastCaptureMask = move.CapturesMask;
    }

    private void DecrementStunned()
    {
        UInt128 stunned = StunnedPieces;
        while (stunned != 0)
        {
            byte position = (byte)BitboardHelpers.BitScanForward(ref stunned);
            _stunnedForPlies[position]--;
            if (_stunnedForPlies[position] == 0)
            {
                StunnedPieces &= ~(UInt128.One << position);
            }
        }
    }

    private void ProcessEnPassant(
        byte from,
        byte to,
        SpecialMoveType specialMoveType,
        BitPiece piece
    )
    {
        EnPassantSquaresMask = 0;
        EnPassantPawnSquare = 0;

        if (
            specialMoveType is not SpecialMoveType.None
            || from == to
            || (GameLogicConstants.PawnLikeMask & (1 << (int)piece.Type)) == 0
        )
        {
            return;
        }

        int fromFile = from % 10;
        int toFile = to % 10;
        if (fromFile != toFile)
        {
            return;
        }

        int fromRank = from / 10;
        int toRank = to / 10;
        int step = (toRank > fromRank) ? 1 : -1;

        int rankDistance = (toRank - fromRank) * step;
        if (
            rankDistance < GameLogicConstants.MinEnPassantTriggerDistance
            || rankDistance > GameLogicConstants.MaxEnPassantTriggerDistance
        )
        {
            return;
        }

        int rank1 = fromRank + step;
        EnPassantSquaresMask |= UInt128.One << (rank1 * 10 + toFile);

        if (rankDistance == 3)
        {
            int rank2 = fromRank + 2 * step;
            EnPassantSquaresMask |= UInt128.One << (rank2 * 10 + toFile);
        }

        EnPassantPawnSquare = to;
    }
}
