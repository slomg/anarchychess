using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitBoardTests
{
    [Fact]
    public void FromPieces_sets_multiple_pieces_and_types_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("d2")] = PieceFactory.White(PieceType.Pawn),
            [new AlgebraicPoint("d3")] = PieceFactory.White(PieceType.SterilePawn),
            [new AlgebraicPoint("d4")] = PieceFactory.White(PieceType.UnderagePawn),
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook),
            [new AlgebraicPoint("h1")] = PieceFactory.White(PieceType.Rook),

            [new AlgebraicPoint("e10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("d9")] = PieceFactory.Black(PieceType.Pawn),
            [new AlgebraicPoint("d8")] = PieceFactory.Black(PieceType.SterilePawn),
            [new AlgebraicPoint("d7")] = PieceFactory.Black(PieceType.UnderagePawn),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
            [new AlgebraicPoint("h10")] = PieceFactory.Black(PieceType.Bishop),

            [new AlgebraicPoint("f4")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new AlgebraicPoint("c5")] = PieceFactory.Neutral(PieceType.TraitorRook),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        // white pieces
        board
            .BitboardFor(PieceType.King, BitPieceColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("e1").AsIdx());

        board
            .BitboardFor(PieceType.Pawn, BitPieceColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d2").AsIdx());

        board
            .BitboardFor(PieceType.SterilePawn, BitPieceColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d3").AsIdx());

        board
            .BitboardFor(PieceType.UnderagePawn, BitPieceColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d4").AsIdx());

        board
            .BitboardFor(PieceType.Rook, BitPieceColor.White)
            .Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("a1").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("h1").AsIdx())
            );

        // black pieces
        board
            .BitboardFor(PieceType.King, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("e10").AsIdx());

        board
            .BitboardFor(PieceType.Pawn, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d9").AsIdx());
        board
            .BitboardFor(PieceType.SterilePawn, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d8").AsIdx());
        board
            .BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d7").AsIdx());

        board
            .BitboardFor(PieceType.Rook, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("a10").AsIdx());
        board
            .BitboardFor(PieceType.Bishop, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("h10").AsIdx());

        // neutral pieces
        board
            .BitboardFor(PieceType.TraitorRook, BitPieceColor.Neutral)
            .Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("f4").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("c5").AsIdx())
            );

        // aggregate checks
        board
            .WhitePieces.Should()
            .Be(
                board.BitboardFor(PieceType.King, BitPieceColor.White)
                    | board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
                    | board.BitboardFor(PieceType.SterilePawn, BitPieceColor.White)
                    | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White)
                    | board.BitboardFor(PieceType.Rook, BitPieceColor.White)
            );
        board
            .BlackPieces.Should()
            .Be(
                board.BitboardFor(PieceType.King, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.Pawn, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.SterilePawn, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.Rook, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.Bishop, BitPieceColor.Black)
            );
        board
            .NeutralPieces.Should()
            .Be(board.BitboardFor(PieceType.TraitorRook, BitPieceColor.Neutral));
        board.Occupancy.Should().Be(board.WhitePieces | board.BlackPieces | board.NeutralPieces);
        board
            .ValidWhiteThrowers.Should()
            .Be(
                board.WhitePieces
                    & ~board.BitboardFor(PieceType.Pawn, BitPieceColor.White)
                    & ~board.BitboardFor(PieceType.SterilePawn, BitPieceColor.White)
                    & ~board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White)
            );
        board
            .ValidBlackThrowers.Should()
            .Be(
                board.BlackPieces
                    & ~board.BitboardFor(PieceType.Pawn, BitPieceColor.Black)
                    & ~board.BitboardFor(PieceType.SterilePawn, BitPieceColor.Black)
                    & ~board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black)
            );

        board
            .WhiteMaterialCount.Should()
            .Be(
                MaterialValue.GetPieceValue(PieceType.King)
                    + MaterialValue.GetPieceValue(PieceType.Pawn)
                    + MaterialValue.GetPieceValue(PieceType.SterilePawn)
                    + MaterialValue.GetPieceValue(PieceType.UnderagePawn)
                    + MaterialValue.GetPieceValue(PieceType.Rook)
                    + MaterialValue.GetPieceValue(PieceType.Rook)
            );
        board
            .BlackMaterialCount.Should()
            .Be(
                MaterialValue.GetPieceValue(PieceType.King)
                    + MaterialValue.GetPieceValue(PieceType.Pawn)
                    + MaterialValue.GetPieceValue(PieceType.SterilePawn)
                    + MaterialValue.GetPieceValue(PieceType.UnderagePawn)
                    + MaterialValue.GetPieceValue(PieceType.Rook)
                    + MaterialValue.GetPieceValue(PieceType.Bishop)
            );
    }

    [Fact]
    public void FromPieces_sets_HasMoved_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.King, hasMoved: true),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        board.HasPieceMoved(new AlgebraicPoint("e1").AsIdx()).Should().BeTrue();
        board.HasPieceMoved(new AlgebraicPoint("a10").AsIdx()).Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FromPieces_sets_IsWhiteToMove(bool isWhiteToMove)
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            isWhiteToMove: isWhiteToMove
        );

        board.IsWhiteToMove.Should().Be(isWhiteToMove);
    }

    [Fact]
    public void FromPieces_sets_can_spawn_omnipotent_pawn_white_to_black_square_capture()
    {
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("h7").AsIdx(),
            To: GameLogicConstants.BlackOmnipotentPawnIdx,
            Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            CaptureMask: GameLogicConstants.BlackOmnipotentPawnMask,
            SpecialMoveType: SpecialMoveType.None
        );

        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [GameLogicConstants.BlackOmnipotentPawnSquare] = PieceFactory.White(PieceType.Pawn),
            },
            isWhiteToMove: false,
            prevMoveState: prevMoveState
        );

        board.CanSpawnOmnipotentPawn.Should().BeTrue();
    }

    [Fact]
    public void FromPieces_sets_can_spawn_omnipotent_pawn_black_to_white_square_capture()
    {
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("h4").AsIdx(),
            To: GameLogicConstants.WhiteOmnipotentPawnIdx,
            Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            CaptureMask: GameLogicConstants.WhiteOmnipotentPawnMask,
            SpecialMoveType: SpecialMoveType.None
        );

        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [GameLogicConstants.WhiteOmnipotentPawnSquare] = PieceFactory.Black(
                    PieceType.Queen
                ),
            },
            isWhiteToMove: true,
            prevMoveState: prevMoveState
        );

        board.CanSpawnOmnipotentPawn.Should().BeTrue();
    }

    [Theory]
    [InlineData("b2", "b4")]
    [InlineData("b2", "b5")]
    public void FromPieces_sets_en_passant_for_correct_pawn_moves(string from, string to)
    {
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint(from).AsIdx(),
            To: new AlgebraicPoint(to).AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CaptureMask: 0,
            SpecialMoveType: SpecialMoveType.None
        );

        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new AlgebraicPoint(from)] = PieceFactory.White(PieceType.Pawn),
            },
            isWhiteToMove: false,
            prevMoveState: prevMoveState
        );

        int toIdx = new AlgebraicPoint(to).AsIdx();

        board.EnPassantPawnSquare.Should().Be(toIdx);

        board.EnPassantSquaresMask.Should().NotBe(0);
    }

    [Fact]
    public void HasPieceMoved_returns_true_for_moved_position_and_false_for_unmoved()
    {
        AlgebraicPoint moved1 = new("a5");
        AlgebraicPoint moved2 = new("d7");
        AlgebraicPoint notMoved = new("g8");
        var board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [moved1] = PieceFactory.White(hasMoved: true),
                [moved2] = PieceFactory.Black(hasMoved: true),
                [notMoved] = PieceFactory.White(hasMoved: false),
            }
        );

        board.HasPieceMoved(moved1.AsIdx()).Should().BeTrue();
        board.HasPieceMoved(moved2.AsIdx()).Should().BeTrue();
        board.HasPieceMoved(notMoved.AsIdx()).Should().BeFalse();
    }

    [Fact]
    public void HasPieceMoved_returns_true_for_masked_bits()
    {
        AlgebraicPoint moved1 = new("a5");
        AlgebraicPoint moved2 = new("d7");
        AlgebraicPoint notMoved = new("g8");
        var board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [moved1] = PieceFactory.White(hasMoved: true),
                [moved2] = PieceFactory.Black(hasMoved: true),
                [notMoved] = PieceFactory.White(hasMoved: false),
            }
        );

        board.HasPieceMoved(UInt128.One << moved1.AsIdx()).Should().BeTrue();
        board.HasPieceMoved(UInt128.One << notMoved.AsIdx()).Should().BeFalse();

        UInt128 mask = (UInt128.One << moved1.AsIdx()) | (UInt128.One << moved2.AsIdx());
        board.HasPieceMoved(mask).Should().BeTrue();

        UInt128 partialMask = (UInt128.One << moved1.AsIdx()) | (UInt128.One << notMoved.AsIdx());
        board.HasPieceMoved(partialMask).Should().BeTrue();
    }

    [Fact]
    public void BitboardForFriendOf_returns_correct_bitboards()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
            [new AlgebraicPoint("f4")] = PieceFactory.Neutral(PieceType.TraitorRook),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        board.BitboardForFriendOf(BitPieceColor.White).Should().Be(board.WhitePieces);
        board.BitboardForFriendOf(BitPieceColor.Black).Should().Be(board.BlackPieces);
        board.BitboardForFriendOf(BitPieceColor.Neutral).Should().Be(0);
    }

    [Fact]
    public void BitboardForEnemyOf_returns_correct_bitboards_including_neutral()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
            [new AlgebraicPoint("f4")] = PieceFactory.Neutral(PieceType.TraitorRook),
        };

        BitBoard board = BitBoard.FromPieces(pieces);

        board
            .BitboardForEnemyOf(BitPieceColor.White)
            .Should()
            .Be(board.BlackPieces | board.NeutralPieces);
        board
            .BitboardForEnemyOf(BitPieceColor.Black)
            .Should()
            .Be(board.WhitePieces | board.NeutralPieces);
        board.BitboardForEnemyOf(BitPieceColor.Neutral).Should().Be(0);
    }

    [Fact]
    public void TryGetPieceAt_returns_true_and_correct_piece_for_occupied_square()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
        };

        var board = BitBoard.FromPieces(pieces);

        bool resultE1 = board.TryGetPieceAt(new AlgebraicPoint("e1").AsIdx(), out var pieceE1);
        resultE1.Should().BeTrue();
        pieceE1.Should().NotBeNull();
        pieceE1.Value.Type.Should().Be(PieceType.King);
        pieceE1.Value.Color.Should().Be(BitPieceColor.White);

        bool resultA10 = board.TryGetPieceAt(new AlgebraicPoint("a10").AsIdx(), out var pieceA10);
        resultA10.Should().BeTrue();
        pieceA10.Should().NotBeNull();
        pieceA10.Value.Type.Should().Be(PieceType.Rook);
        pieceA10.Value.Color.Should().Be(BitPieceColor.Black);
    }

    [Fact]
    public void TryGetPieceAt_returns_false_for_empty_square()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.King),
        };

        var board = BitBoard.FromPieces(pieces);

        bool result = board.TryGetPieceAt(new AlgebraicPoint("a1").AsIdx(), out var piece);
        result.Should().BeFalse();
        piece.Should().BeNull();
    }

    [Fact]
    public void TryGetPieceAt_works_for_neutral_piece()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f4")] = PieceFactory.Neutral(PieceType.TraitorRook),
        };

        var board = BitBoard.FromPieces(pieces);

        bool result = board.TryGetPieceAt(new AlgebraicPoint("f4").AsIdx(), out var piece);
        result.Should().BeTrue();
        piece.Should().NotBeNull();
        piece.Value.Type.Should().Be(PieceType.TraitorRook);
        piece.Value.Color.Should().Be(BitPieceColor.Neutral);
    }

    [Fact]
    public void MakeNullMove_flips_turn_and_resets_move_state()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b2")] = PieceFactory.White(PieceType.Pawn),
            [new AlgebraicPoint("a5")] = PieceFactory.Black(PieceType.Pawn),
        };
        Dictionary<AlgebraicPoint, int> stunnedPositions = new()
        {
            [new("b2")] = 1,
            [new("a5")] = 2,
        };

        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("b2").AsIdx(),
            To: new AlgebraicPoint("b4").AsIdx(),
            Piece: new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: UInt128.One << new AlgebraicPoint("h3").AsIdx(),
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            pieces,
            isWhiteToMove: true,
            stunnedPositions: stunnedPositions,
            prevMoveState: prevMoveState
        );

        byte enPassantPawnSquare = new AlgebraicPoint("b4").AsIdx();
        UInt128 enPassantSquaresMask = UInt128.One << new AlgebraicPoint("b3").AsIdx();
        bool canSpawnOmnipotentPawn = true;
        UInt128 stunnedPieces =
            (UInt128.One << new AlgebraicPoint("b2").AsIdx())
            | (UInt128.One << new AlgebraicPoint("a5").AsIdx());
        ulong zobristKey = board.ZobristKey;

        board.EnPassantPawnSquare.Should().Be(enPassantPawnSquare);
        board.EnPassantSquaresMask.Should().Be(enPassantSquaresMask);
        board.CanSpawnOmnipotentPawn.Should().Be(canSpawnOmnipotentPawn);
        board.StunnedPieces.Should().Be(stunnedPieces);

        var undo = board.MakeNullMove();

        board.IsWhiteToMove.Should().BeFalse();
        board.EnPassantPawnSquare.Should().Be(0);
        board.EnPassantSquaresMask.Should().Be(0);
        board.CanSpawnOmnipotentPawn.Should().Be(false);
        board.StunnedPieces.Should().Be(UInt128.One << new AlgebraicPoint("a5").AsIdx());
        board
            .ZobristKey.Should()
            .Be(
                zobristKey
                    ^ Zobrist.SideToMove
                    ^ Zobrist.EnPassantSquare[enPassantPawnSquare]
                    ^ Zobrist.CanSpawnOmnipotentPawn
                    ^ Zobrist.StunnedForPlies[new AlgebraicPoint("b2").AsIdx(), 1]
                    ^ Zobrist.StunnedForPlies[new AlgebraicPoint("a5").AsIdx(), 2]
                    ^ Zobrist.StunnedForPlies[new AlgebraicPoint("a5").AsIdx(), 1]
            );

        BitBoard expectedPiecesBoard = BitBoard.FromPieces(pieces);
        board
            .Should()
            .BeEquivalentTo(
                expectedPiecesBoard,
                options =>
                    options
                        .Excluding(x => x.IsWhiteToMove)
                        .Excluding(x => x.EnPassantPawnSquare)
                        .Excluding(x => x.EnPassantSquaresMask)
                        .Excluding(x => x.CanSpawnOmnipotentPawn)
                        .Excluding(x => x.StunnedForPlies)
                        .Excluding(x => x.StunnedPieces)
                        .Excluding(x => x.ZobristKey)
            );

        undo.Should()
            .BeEquivalentTo(
                new NullMoveUndoState()
                {
                    IsWhiteToMove = true,
                    EnPassantPawnSquare = enPassantPawnSquare,
                    EnPassantSquaresMask = enPassantSquaresMask,
                    CanSpawnOmnipotentPawn = canSpawnOmnipotentPawn,
                    ZobristKey = zobristKey,
                    StunnedPieces = stunnedPieces,
                }
            );
    }

    [Fact]
    public void UndoNullMove_restores_previous_board_state()
    {
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("b2").AsIdx(),
            To: new AlgebraicPoint("b4").AsIdx(),
            Piece: new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CaptureMask: UInt128.One << new AlgebraicPoint("a5").AsIdx(),
            SpecialMoveType: SpecialMoveType.None
        );
        Dictionary<AlgebraicPoint, int> stunnedPositions = new() { [new AlgebraicPoint("b2")] = 1 };
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b2")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(
            pieces,
            stunnedPositions: stunnedPositions,
            prevMoveState: prevMoveState
        );
        BitBoard original = BitBoard.FromPieces(
            pieces,
            stunnedPositions: stunnedPositions,
            prevMoveState: prevMoveState
        );

        var undo = board.MakeNullMove();
        board.UndoNullMove(undo);

        board.Should().BeEquivalentTo(original);
    }
}
