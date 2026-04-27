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
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            isWhiteToMove: true
        );
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(0);
    }

    [Fact]
    public void GenerateMoves_adds_white_pawn_when_white_to_move_and_last_capture_on_white_square()
    {
        PrevMoveState prevMove = new(
            From: 0,
            To: GameLogicConstants.WhiteOmnipotentPawnIdx,
            Piece: new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.Black },
            CaptureMask: GameLogicConstants.WhiteOmnipotentPawnMask | (UInt128.One << 69),
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            isWhiteToMove: true,
            prevMoveState: prevMove
        );
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(1);
        moves[0]
            .Should()
            .BeEquivalentTo(
                new BitMove
                {
                    From = GameLogicConstants.WhiteOmnipotentPawnIdx,
                    To = GameLogicConstants.WhiteOmnipotentPawnIdx,
                    Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
                    CapturesMask = GameLogicConstants.WhiteOmnipotentPawnMask,
                    SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
                }
            );
    }

    [Fact]
    public void GenerateMoves_adds_black_pawn_when_black_to_move_and_last_capture_on_black_square()
    {
        PrevMoveState prevMove = new(
            From: 0,
            To: GameLogicConstants.BlackOmnipotentPawnIdx,
            Piece: new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
            CaptureMask: GameLogicConstants.BlackOmnipotentPawnMask | (UInt128.One << 69),
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            isWhiteToMove: false,
            prevMoveState: prevMove
        );
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(1);
        moves[0]
            .Should()
            .BeEquivalentTo(
                new BitMove
                {
                    From = GameLogicConstants.BlackOmnipotentPawnIdx,
                    To = GameLogicConstants.BlackOmnipotentPawnIdx,
                    Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.Black },
                    CapturesMask = GameLogicConstants.BlackOmnipotentPawnMask,
                    SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
                }
            );
    }

    [Fact]
    public void GenerateMoves_does_not_add_white_pawn_if_last_capture_not_on_white_square()
    {
        PrevMoveState prevMove = new(
            From: 0,
            To: GameLogicConstants.BlackOmnipotentPawnIdx,
            Piece: new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.Black },
            CaptureMask: GameLogicConstants.BlackOmnipotentPawnMask,
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            isWhiteToMove: true,
            prevMoveState: prevMove
        );
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(0);
    }

    [Fact]
    public void GenerateMoves_does_not_add_black_pawn_if_last_capture_not_on_black_square()
    {
        PrevMoveState prevMove = new(
            From: 0,
            To: GameLogicConstants.WhiteOmnipotentPawnIdx,
            Piece: new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
            CaptureMask: GameLogicConstants.WhiteOmnipotentPawnMask,
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            isWhiteToMove: false,
            prevMoveState: prevMove
        );
        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _rule.GenerateMoves(board, moves, ref moveCount);

        moveCount.Should().Be(0);
    }
}
