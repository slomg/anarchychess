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

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    [InlineData(PieceType.SterilePawn)]
    public void FromPieces_sets_EnPassantSquares_correctly_for_pawn_moves(PieceType pawnType)
    {
        var from = new AlgebraicPoint("b2").AsIdx();
        var to = new AlgebraicPoint("b5").AsIdx();

        BitMove move = new()
        {
            Piece = pawnType,
            From = from,
            To = to,
        };

        var board = BitBoard.FromPieces(
            new() { [new AlgebraicPoint("b2")] = PieceFactory.White(pawnType) },
            isWhiteToMove: true,
            prevMove: move
        );

        var expectedEnPassantSquare =
            (UInt128.One << new AlgebraicPoint("b3").AsIdx())
            | (UInt128.One << new AlgebraicPoint("b4").AsIdx());
        board.EnPassantSquares.Should().Be(expectedEnPassantSquare);
        board.EnPassantPawnSquare.Should().Be(move.To);
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    [InlineData(PieceType.SterilePawn)]
    public void FromPieces_sets_EnPassantSquares_correctly_for_black_pawn(PieceType pawnType)
    {
        var from = new AlgebraicPoint("c9").AsIdx();
        var to = new AlgebraicPoint("c6").AsIdx();

        BitMove move = new()
        {
            Piece = pawnType,
            From = from,
            To = to,
        };

        var board = BitBoard.FromPieces(
            new() { [new AlgebraicPoint("c9")] = PieceFactory.Black(pawnType) },
            isWhiteToMove: false,
            prevMove: move
        );

        var expectedEnPassantSquare =
            (UInt128.One << new AlgebraicPoint("c8").AsIdx())
            | (UInt128.One << new AlgebraicPoint("c7").AsIdx());
        board.EnPassantSquares.Should().Be(expectedEnPassantSquare);
    }

    [Fact]
    public void HasPieceMoved_returns_true_for_moved_position_and_false_for_unmoved()
    {
        var board = new BitBoard(hasMoved: (UInt128.One << 5) | (UInt128.One << 10));

        board.HasPieceMoved(5).Should().BeTrue();
        board.HasPieceMoved(10).Should().BeTrue();
        board.HasPieceMoved(3).Should().BeFalse();
    }

    [Fact]
    public void HasPieceMoved_returns_true_for_masked_bits()
    {
        var board = new BitBoard(hasMoved: (UInt128.One << 3) | (UInt128.One << 7));

        board.HasPieceMoved(UInt128.One << 3).Should().BeTrue();
        board.HasPieceMoved(UInt128.One << 5).Should().BeFalse();

        UInt128 mask = (UInt128.One << 3) | (UInt128.One << 7);
        board.HasPieceMoved(mask).Should().BeTrue();

        UInt128 partialMask = (UInt128.One << 3) | (UInt128.One << 5);
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
        pieceE1.Value.PieceType.Should().Be(PieceType.King);
        pieceE1.Value.Color.Should().Be(BitPieceColor.White);

        bool resultA10 = board.TryGetPieceAt(new AlgebraicPoint("a10").AsIdx(), out var pieceA10);
        resultA10.Should().BeTrue();
        pieceA10.Should().NotBeNull();
        pieceA10.Value.PieceType.Should().Be(PieceType.Rook);
        pieceA10.Value.Color.Should().Be(BitPieceColor.Black);
    }

    [Fact]
    public void TryGetPieceAt_returns_false_for_empty_square()
    {
        var pieces = new Dictionary<AlgebraicPoint, Piece>
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
        var pieces = new Dictionary<AlgebraicPoint, Piece>
        {
            [new AlgebraicPoint("f4")] = PieceFactory.Neutral(PieceType.TraitorRook),
        };

        var board = BitBoard.FromPieces(pieces);

        bool result = board.TryGetPieceAt(new AlgebraicPoint("f4").AsIdx(), out var piece);
        result.Should().BeTrue();
        piece.Should().NotBeNull();
        piece.Value.PieceType.Should().Be(PieceType.TraitorRook);
        piece.Value.Color.Should().Be(BitPieceColor.Neutral);
    }
}
