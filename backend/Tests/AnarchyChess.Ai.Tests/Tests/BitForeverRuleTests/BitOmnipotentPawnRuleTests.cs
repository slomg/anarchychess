using AnarchyChess.Ai.BitForeverRules;
using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.BitForeverRuleTests;

public class BitOmnipotentPawnRuleTests
{
    private readonly BitOmnipotentPawnRule _rule = new();

    [Fact]
    public void GenerateMoves_does_nothing_when_last_capture_mask_is_zero()
    {
        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: true, prevMove: null);
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(0);
    }

    [Fact]
    public void GenerateMoves_adds_white_pawn_when_white_to_move_and_last_capture_on_white_square()
    {
        BitMove prevMove = new()
        {
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.Black },
            From = 0,
            To = BitOmnipotentPawnRule.WhiteSquare,
            CapturesMask = BitOmnipotentPawnRule.WhiteSquareMask,
        };

        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: true, prevMove: prevMove);
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(1);
        moves[0]
            .Should()
            .BeEquivalentTo(
                new BitMove
                {
                    From = BitOmnipotentPawnRule.WhiteSquare,
                    To = BitOmnipotentPawnRule.WhiteSquare,
                    Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
                    CapturesMask = BitOmnipotentPawnRule.WhiteSquareMask,
                    SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
                }
            );
    }

    [Fact]
    public void GenerateMoves_adds_black_pawn_when_black_to_move_and_last_capture_on_black_square()
    {
        BitMove prevMove = new()
        {
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
            From = 0,
            To = BitOmnipotentPawnRule.BlackSquare,
            CapturesMask = BitOmnipotentPawnRule.BlackSquareMask,
        };

        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: false, prevMove: prevMove);
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(1);
        moves[0]
            .Should()
            .BeEquivalentTo(
                new BitMove
                {
                    From = BitOmnipotentPawnRule.BlackSquare,
                    To = BitOmnipotentPawnRule.BlackSquare,
                    Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.Black },
                    CapturesMask = BitOmnipotentPawnRule.BlackSquareMask,
                    SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
                }
            );
    }

    [Fact]
    public void GenerateMoves_does_not_add_white_pawn_if_last_capture_not_on_white_square()
    {
        BitMove prevMove = new()
        {
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.Black },
            From = 0,
            To = BitOmnipotentPawnRule.BlackSquare,
            CapturesMask = BitOmnipotentPawnRule.BlackSquareMask,
        };

        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: true, prevMove: prevMove);
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(0);
    }

    [Fact]
    public void GenerateMoves_does_not_add_black_pawn_if_last_capture_not_on_black_square()
    {
        BitMove prevMove = new()
        {
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
            From = 0,
            To = BitOmnipotentPawnRule.WhiteSquare,
            CapturesMask = BitOmnipotentPawnRule.WhiteSquare,
        };

        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: false, prevMove: prevMove);
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(0);
    }
}
