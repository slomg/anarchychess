using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Game;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitboardZobristTests
{
    [Fact]
    public void ZobristKey_is_equivalent_for_the_same_position_reached_different_ways()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard boardA = BitBoard.FromPieces(pieces);
        boardA.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e1").AsIdx(),
                To = new AlgebraicPoint("e2").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );
        boardA.ZobristKey.Should().Be(Zobrist.Compute(boardA));

        boardA.MakeNullMove();
        boardA.ZobristKey.Should().Be(Zobrist.Compute(boardA));

        boardA.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e2").AsIdx(),
                To = new AlgebraicPoint("e3").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );
        boardA.ZobristKey.Should().Be(Zobrist.Compute(boardA));

        BitBoard boardB = BitBoard.FromPieces(pieces);
        boardB.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e1").AsIdx(),
                To = new AlgebraicPoint("e3").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );

        boardA.ZobristKey.Should().Be(boardB.ZobristKey);
    }

    [Fact]
    public void ZobristKey_is_correct_after_clone()
    {
        BitBoard board = new(BitBoard.FromPieces(GameConstants.StartingPosition));

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_remove_piece()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e10")] = PieceFactory.White(PieceType.Rook),
            [new("e5")] = PieceFactory.Black(),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e10").AsIdx(),
                To = new AlgebraicPoint("e5").AsIdx(),
                Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
                CapturesMask = UInt128.One << new AlgebraicPoint("e5").AsIdx(),
            }
        );

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_move_piece()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King, hasMoved: false),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e1").AsIdx(),
                To = new AlgebraicPoint("e2").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_move_piece_that_has_already_moved()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.King, hasMoved: true),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e2").AsIdx(),
                To = new AlgebraicPoint("e3").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_null_move()
    {
        BitBoard board = BitBoard.FromPieces(GameConstants.StartingPosition);
        board.MakeNullMove();

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_null_move_with_en_passant()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.Black(PieceType.Pawn),
        };
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("e2").AsIdx(),
            To: new AlgebraicPoint("e5").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: 0,
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState);
        board.EnPassantPawnSquare.Should().NotBe(0);
        board.MakeNullMove();
        board.EnPassantPawnSquare.Should().Be(0);

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_null_move_with_omnipotent_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("h3")] = PieceFactory.Black(PieceType.Queen),
        };
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("h4").AsIdx(),
            To: new AlgebraicPoint("h3").AsIdx(),
            Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            CaptureMask: UInt128.One << new AlgebraicPoint("h3").AsIdx(),
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState);
        board.CanSpawnOmnipotentPawn.Should().Be(true);
        board.MakeNullMove();
        board.CanSpawnOmnipotentPawn.Should().Be(false);

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_en_passant_is_set()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e2").AsIdx(),
                To = new AlgebraicPoint("e5").AsIdx(),
                Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            }
        );
        board.EnPassantPawnSquare.Should().NotBe(0);

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_en_passant_is_cleared()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e6")] = PieceFactory.Black(PieceType.Pawn),
            [new("a1")] = PieceFactory.White(PieceType.King),
        };
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("e8").AsIdx(),
            To: new AlgebraicPoint("e6").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: 0,
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState);
        board.EnPassantPawnSquare.Should().NotBe(0);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("a1").AsIdx(),
                To = new AlgebraicPoint("a2").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );
        board.EnPassantPawnSquare.Should().Be(0);

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_omnipotent_pawn_triggered()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("h5")] = PieceFactory.White(PieceType.Queen),
            [new("h8")] = PieceFactory.Black(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("h5").AsIdx(),
                To = new AlgebraicPoint("h8").AsIdx(),
                Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
                CapturesMask = UInt128.One << new AlgebraicPoint("h8").AsIdx(),
            }
        );
        board.CanSpawnOmnipotentPawn.Should().BeTrue();

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_omnipotent_pawn_cleared()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("h8")] = PieceFactory.White(PieceType.Queen),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("h7").AsIdx(),
            To: new AlgebraicPoint("h8").AsIdx(),
            Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            CaptureMask: UInt128.One << new AlgebraicPoint("h8").AsIdx(),
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            pieces,
            prevMoveState: prevMoveState,
            isWhiteToMove: false
        );
        board.CanSpawnOmnipotentPawn.Should().BeTrue();
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e10").AsIdx(),
                To = new AlgebraicPoint("e9").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.Black },
            }
        );
        board.CanSpawnOmnipotentPawn.Should().BeFalse();

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_stun_decrements()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        Dictionary<AlgebraicPoint, int> stunnedPositions = new() { [new("e1")] = 2 };
        BitBoard board = BitBoard.FromPieces(pieces, stunnedPositions: stunnedPositions);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e1").AsIdx(),
                To = new AlgebraicPoint("e2").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_stun_expires()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        Dictionary<AlgebraicPoint, int> stunnedPositions = new() { [new("e1")] = 1 };
        BitBoard board = BitBoard.FromPieces(pieces, stunnedPositions: stunnedPositions);
        board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e1").AsIdx(),
                To = new AlgebraicPoint("e2").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );

        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_undo_move()
    {
        BitBoard board = BitBoard.FromPieces(GameConstants.StartingPosition);
        ulong keyBefore = board.ZobristKey;
        var undo = board.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e2").AsIdx(),
                To = new AlgebraicPoint("e3").AsIdx(),
                Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            }
        );
        board.UndoMove(undo);

        board.ZobristKey.Should().Be(keyBefore);
        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }

    [Fact]
    public void ZobristKey_is_correct_after_undo_null_move()
    {
        BitBoard board = BitBoard.FromPieces(GameConstants.StartingPosition);
        ulong keyBefore = board.ZobristKey;
        var undo = board.MakeNullMove();
        board.UndoNullMove(undo);

        board.ZobristKey.Should().Be(keyBefore);
        board.ZobristKey.Should().Be(Zobrist.Compute(board));
    }
}
