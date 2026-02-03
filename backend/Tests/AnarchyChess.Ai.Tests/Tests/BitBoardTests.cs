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
            .BitboardFor(BitPiece.King, GameColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("e1").AsIdx());

        board
            .BitboardFor(BitPiece.Queen, GameColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d1").AsIdx());

        board
            .BitboardFor(BitPiece.Rook, GameColor.White)
            .Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("a1").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("h1").AsIdx())
            );

        // black pieces
        board
            .BitboardFor(BitPiece.King, GameColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("e10").AsIdx());

        board
            .BitboardFor(BitPiece.Queen, GameColor.Black)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("d10").AsIdx());

        board
            .BitboardFor(BitPiece.Rook, GameColor.Black)
            .Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("a10").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("h10").AsIdx())
            );

        // neutral pieces
        board
            .BitboardFor(NeutralBitPiece.TraitorRook)
            .Should()
            .Be(
                (UInt128.One << new AlgebraicPoint("f4").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("c5").AsIdx())
            );

        // aggregate checks
        board
            .WhitePieces.Should()
            .Be(
                board.BitboardFor(BitPiece.King, GameColor.White)
                    | board.BitboardFor(BitPiece.Queen, GameColor.White)
                    | board.BitboardFor(BitPiece.Rook, GameColor.White)
            );

        board
            .BlackPieces.Should()
            .Be(
                board.BitboardFor(BitPiece.King, GameColor.Black)
                    | board.BitboardFor(BitPiece.Queen, GameColor.Black)
                    | board.BitboardFor(BitPiece.Rook, GameColor.Black)
            );

        board.NeutralPieces.Should().Be(board.BitboardFor(NeutralBitPiece.TraitorRook));

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

        board.BitboardForFriendOf(GameColor.White).Should().Be(board.WhitePieces);
        board.BitboardForFriendOf(GameColor.Black).Should().Be(board.BlackPieces);
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
            .BitboardForEnemyOf(GameColor.White)
            .Should()
            .Be(board.BlackPieces | board.NeutralPieces);
        board
            .BitboardForEnemyOf(GameColor.Black)
            .Should()
            .Be(board.WhitePieces | board.NeutralPieces);
    }
}
