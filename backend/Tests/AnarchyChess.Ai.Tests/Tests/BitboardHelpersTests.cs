using AnarchyChess.Ai.Helpers;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitboardHelpersTests
{
    [Fact]
    public void BitScanForward_returns_the_index_of_the_least_significant_bit()
    {
        UInt128 bitboard = 0b10010;
        int index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(1);
        bitboard.Should().Be(0b10000);
    }

    [Fact]
    public void BitScanForward_handles_a_single_bit_set()
    {
        UInt128 bitboard = 0b10000000;
        int index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(7);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanForward_handles_high_bit_set()
    {
        UInt128 bitboard = UInt128.One << 67;
        int index = BitboardHelpers.BitScanForward(ref bitboard);

        index.Should().Be(67);
        bitboard.Should().Be(0);
    }

    [Fact]
    public void BitScanForward_throws_for_an_empty_bitboard()
    {
        UInt128 bitboard = 0;

        Action act = () => BitboardHelpers.BitScanForward(ref bitboard);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0UL, 0UL, 0)] // empty
    [InlineData(1UL << 3, 0UL, 1)] // low half only
    [InlineData(0UL, 1UL << 5, 1)] // high half only
    [InlineData(1UL << 1, 1UL << 1, 2)] // both halves
    [InlineData(1UL << 63, 1UL << 0, 2)] // boundary correctness
    public void CountBits_adds_low_and_high_halves_correctly(ulong low, ulong high, int expected)
    {
        UInt128 mask = ((UInt128)high << 64) | low;

        BitboardHelpers.CountBits(mask).Should().Be(expected);
    }

    [Fact]
    public void MaskAdjacent_sets_correct_adjacent_bits_in_middle()
    {
        byte position = 21; // somewhere in the middle of the board
        UInt128 result = BitboardHelpers.MaskAdjacent(position);

        UInt128 expected =
            (UInt128.One << 20)
            | // left
            (UInt128.One << 22)
            | // right
            (UInt128.One << 11)
            | // up
            (UInt128.One << 31)
            | // down
            (UInt128.One << 10)
            | // up left
            (UInt128.One << 12)
            | // up right
            (UInt128.One << 30)
            | // down left
            (UInt128.One << 32); // down right

        result.Should().Be(expected);
    }

    [Fact]
    public void MaskAdjacent_does_not_wrap_around_edges()
    {
        byte position = 0;
        UInt128 result = BitboardHelpers.MaskAdjacent(position);

        UInt128 expected =
            (UInt128.One << 1)
            | // right
            (UInt128.One << 10)
            | // up
            (UInt128.One << 11); // up-right

        result.Should().Be(expected);
    }

    [Fact]
    public void CreateMoveFromAttacks_handles_mixed_quiet_and_capture()
    {
        var capturedPiece = PieceFactory.White();
        BitBoard board = BitBoard.FromPieces(new() { [new("c1")] = capturedPiece });

        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        PieceType piece = PieceType.Rook;
        UInt128 attacks = (UInt128.One << 1) | (UInt128.One << 2);

        BitboardHelpers.CreateMoveFromAttacks(
            from,
            piece,
            board,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );

        BitMove quietMove = new()
        {
            From = 0,
            To = 1,
            Piece = piece,
        };
        BitMove captureMove = new()
        {
            From = 0,
            To = 2,
            Piece = piece,
        };
        captureMove.AddCapture(square: 2, piece: capturedPiece.Type, color: BitPieceColor.White);

        List<BitMove> expectedMoves = [quietMove, captureMove];
        List<BitMove> result = [.. moves[..moveCount]];

        result.Should().BeEquivalentTo(expectedMoves);
    }

    [Fact]
    public void CreateMoveFromQuiets_creates_moves_for_each_quiet_square()
    {
        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        PieceType piece = PieceType.Rook;
        UInt128 quiets = (UInt128.One << 2) | (UInt128.One << 5);

        BitboardHelpers.CreateMoveFromQuiets(from, piece, quiets, moves, ref moveCount);

        BitMove[] expectedMoves =
        [
            new BitMove
            {
                From = 0,
                To = 2,
                Piece = piece,
            },
            new BitMove
            {
                From = 0,
                To = 5,
                Piece = piece,
            },
        ];

        List<BitMove> result = [.. moves[..moveCount]];
        result.Should().BeEquivalentTo(expectedMoves);
    }

    [Fact]
    public void CreateMoveFromCaptures_creates_moves_for_each_capture()
    {
        var capturedPiece1 = PieceFactory.White(PieceType.Pawn);
        AlgebraicPoint capturedPiece1Position = new("b1");
        byte capturedPiece1Idx = capturedPiece1Position.AsIdx();

        var capturedPiece2 = PieceFactory.Black(PieceType.Horsey);
        AlgebraicPoint capturedPiece2Position = new("c2");
        byte capturedPiece2Idx = capturedPiece2Position.AsIdx();

        BitBoard board = BitBoard.FromPieces(
            new()
            {
                [capturedPiece1Position] = capturedPiece1,
                [capturedPiece2Position] = capturedPiece2,
            }
        );

        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        byte from = 0;
        PieceType piece = PieceType.Rook;
        UInt128 captures = (UInt128.One << capturedPiece1Idx) | (UInt128.One << capturedPiece2Idx);

        BitboardHelpers.CreateMoveFromCaptures(from, piece, board, captures, moves, ref moveCount);

        BitMove expectedMove1 = new()
        {
            From = from,
            To = capturedPiece1Idx,
            Piece = piece,
        };
        expectedMove1.AddCapture(capturedPiece1Idx, capturedPiece1.Type, BitPieceColor.White);

        BitMove expectedMove2 = new()
        {
            From = from,
            To = capturedPiece2Idx,
            Piece = piece,
        };
        expectedMove2.AddCapture(capturedPiece2Idx, capturedPiece2.Type, BitPieceColor.Black);

        List<BitMove> expectedMoves = [expectedMove1, expectedMove2];
        List<BitMove> result = [.. moves[..moveCount]];

        result.Should().BeEquivalentTo(expectedMoves);
    }

    [Fact]
    public void CreateMoveFromCaptures_does_nothing_when_no_captures()
    {
        BitBoard board = new();
        Span<BitMove> moves = new BitMove[10];
        int moveCount = 0;

        BitboardHelpers.CreateMoveFromCaptures(
            0,
            PieceType.Rook,
            board,
            captures: UInt128.One << 10,
            moves,
            ref moveCount
        );

        moveCount.Should().Be(0);
    }
}
