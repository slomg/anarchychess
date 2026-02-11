using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BoardHasherTests
{
    private readonly BoardHasher _hasher = new();

    [Fact]
    public void CalculateHash_is_deterministic_for_same_board()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b2")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        UInt128 hash1 = _hasher.CalculateHash(board);
        UInt128 hash2 = _hasher.CalculateHash(board);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void CalculateHash_changes_when_piece_moves()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        UInt128 hashBefore = _hasher.CalculateHash(board);
        board.MakeMove(
            new BitMove()
            {
                From = new AlgebraicPoint("a1").AsIdx(),
                To = new AlgebraicPoint("a2").AsIdx(),
                Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
            }
        );
        UInt128 hashAfter = _hasher.CalculateHash(board);

        hashBefore.Should().NotBe(hashAfter);
    }

    [Fact]
    public void CalculateHash_is_different_for_different_boards()
    {
        BitBoard board1 = BitBoard.FromPieces(
            new() { [new("a1")] = PieceFactory.White(PieceType.Rook) }
        );
        BitBoard board2 = BitBoard.FromPieces(
            new() { [new("b1")] = PieceFactory.White(PieceType.Rook) }
        );

        UInt128 hash1 = _hasher.CalculateHash(board1);
        UInt128 hash2 = _hasher.CalculateHash(board2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void CalculateHash_handles_empty_board()
    {
        var board = new BitBoard();
        UInt128 hash = _hasher.CalculateHash(board);
        hash.Should().NotBe(0);
    }

    [Fact]
    public void CalculateHash_is_different_for_the_same_pieces_but_different_side_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b2")] = PieceFactory.Black(PieceType.Pawn),
        };

        BitBoard board1 = BitBoard.FromPieces(pieces, isWhiteToMove: true);
        BitBoard board2 = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        UInt128 hash1 = _hasher.CalculateHash(board1);
        UInt128 hash2 = _hasher.CalculateHash(board2);

        hash1.Should().NotBe(hash2);
    }
}
