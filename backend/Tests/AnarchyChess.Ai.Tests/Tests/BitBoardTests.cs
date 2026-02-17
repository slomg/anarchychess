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
            [new AlgebraicPoint("d1")] = PieceFactory.White(PieceType.Queen),
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook),
            [new AlgebraicPoint("h1")] = PieceFactory.White(PieceType.Rook),

            [new AlgebraicPoint("e10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("d10")] = PieceFactory.Black(PieceType.Queen),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
            [new AlgebraicPoint("h10")] = PieceFactory.Black(PieceType.Rook),

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
            .BitboardFor(PieceType.Queen, BitPieceColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d1").AsIdx());

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
            .BitboardFor(PieceType.Queen, BitPieceColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d10").AsIdx());

        board
            .BitboardFor(PieceType.Rook, BitPieceColor.Black)
            .Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("a10").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("h10").AsIdx())
            );

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
                    | board.BitboardFor(PieceType.Queen, BitPieceColor.White)
                    | board.BitboardFor(PieceType.Rook, BitPieceColor.White)
            );

        board
            .BlackPieces.Should()
            .Be(
                board.BitboardFor(PieceType.King, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.Queen, BitPieceColor.Black)
                    | board.BitboardFor(PieceType.Rook, BitPieceColor.Black)
            );

        board
            .NeutralPieces.Should()
            .Be(board.BitboardFor(PieceType.TraitorRook, BitPieceColor.Neutral));

        board.Occupancy.Should().Be(board.WhitePieces | board.BlackPieces | board.NeutralPieces);
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
        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: isWhiteToMove);

        board.IsWhiteToMove.Should().Be(isWhiteToMove);
    }

    [Fact]
    public void FromPieces_sets_last_move_state()
    {
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("b2").AsIdx(),
            To: new AlgebraicPoint("b5").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CaptureMask: (UInt128.One << 1) | (UInt128.One << 15)
        );

        BitBoard board = BitBoard.FromPieces(
            new() { [new AlgebraicPoint("b2")] = PieceFactory.White(PieceType.Pawn) },
            isWhiteToMove: true,
            prevMoveState
        );

        board
            .EnPassantSquaresMask.Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("b3").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("b4").AsIdx())
            );
        board.EnPassantPawnSquare.Should().Be(prevMoveState.To);
        board.LastCaptureMask.Should().Be(prevMoveState.CaptureMask);
    }

    [Fact]
    public void HasPieceMoved_returns_true_for_moved_position_and_false_for_unmoved()
    {
        AlgebraicPoint moved1 = new("a5");
        AlgebraicPoint moved2 = new("d7");
        AlgebraicPoint notMoved = new("g8");
        var board = BitBoard.FromPieces(
            new()
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
            new()
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

        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("b2").AsIdx(),
            To: new AlgebraicPoint("b4").AsIdx(),
            Piece: new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CaptureMask: UInt128.One << new AlgebraicPoint("a5").AsIdx()
        );
        BitBoard board = BitBoard.FromPieces(
            pieces,
            isWhiteToMove: true,
            prevMoveState: prevMoveState
        );

        byte enPassantPawnSquare = new AlgebraicPoint("b4").AsIdx();
        UInt128 enPassantSquaresMask = UInt128.One << new AlgebraicPoint("b3").AsIdx();
        UInt128 lastCaptureMask = UInt128.One << new AlgebraicPoint("a5").AsIdx();

        board.EnPassantPawnSquare.Should().Be(enPassantPawnSquare);
        board.EnPassantSquaresMask.Should().Be(enPassantSquaresMask);
        board.LastCaptureMask.Should().Be(lastCaptureMask);

        var undo = board.MakeNullMove();

        board.IsWhiteToMove.Should().BeFalse();
        board.EnPassantPawnSquare.Should().Be(0);
        board.EnPassantSquaresMask.Should().Be(0);
        board.LastCaptureMask.Should().Be(0);

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
                        .Excluding(x => x.LastCaptureMask)
            );

        undo.Should()
            .BeEquivalentTo(
                new NullMoveUndoState()
                {
                    PrevIsWhiteToMove = true,
                    PrevEnPassantPawnSquare = enPassantPawnSquare,
                    PrevEnPassantSquaresMask = enPassantSquaresMask,
                    PrevLastCaptureMask = lastCaptureMask,
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
            CaptureMask: UInt128.One << new AlgebraicPoint("a5").AsIdx()
        );
        BitBoard board = BitBoard.FromPieces(
            new() { [new AlgebraicPoint("b2")] = PieceFactory.White(PieceType.Pawn) },
            isWhiteToMove: true,
            prevMoveState
        );

        var undo = board.MakeNullMove();
        board.UndoNullMove(undo);

        board.IsWhiteToMove.Should().BeTrue();
        board.EnPassantPawnSquare.Should().Be(new AlgebraicPoint("b4").AsIdx());
        board.EnPassantSquaresMask.Should().Be(UInt128.One << new AlgebraicPoint("b3").AsIdx());
        board.LastCaptureMask.Should().Be(UInt128.One << new AlgebraicPoint("a5").AsIdx());
    }
}
