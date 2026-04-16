using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitboardUndoMoveTests
{
    [Fact]
    public void UndoMove_restores_simple_move()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("a2").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        AssertMoveUndo(new() { [new("a1")] = PieceFactory.White(PieceType.Rook) }, move);
    }

    [Fact]
    public void UndoMove_restores_captures()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("d1").AsIdx(),
            To = new AlgebraicPoint("d9").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask =
                (UInt128.One << new AlgebraicPoint("d10").AsIdx())
                | (UInt128.One << new AlgebraicPoint("d9").AsIdx())
                | (UInt128.One << new AlgebraicPoint("e8").AsIdx()),
        };
        AssertMoveUndo(
            new()
            {
                [new("d1")] = PieceFactory.White(PieceType.Queen),
                [new("d10")] = PieceFactory.Black(),
                [new("d9")] = PieceFactory.Black(),
                [new("e8")] = PieceFactory.Black(),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_promotion()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("b9").AsIdx(),
            To = new AlgebraicPoint("b10").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };
        AssertMoveUndo(new() { [new("b9")] = PieceFactory.White(PieceType.Pawn) }, move);
    }

    [Fact]
    public void UndoMove_restores_castle_capture()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("h1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
            CapturesMask = UInt128.One << new AlgebraicPoint("g1").AsIdx(),
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
                [new AlgebraicPoint("g1")] = PieceFactory.White(PieceType.Bishop),
                [new AlgebraicPoint("j1")] = PieceFactory.White(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_double_castle_capture()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("h1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
            CapturesMask =
                (UInt128.One << new AlgebraicPoint("g1").AsIdx())
                | UInt128.One << new AlgebraicPoint("h1").AsIdx(),
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
                [new AlgebraicPoint("g1")] = PieceFactory.White(PieceType.Bishop),
                [new AlgebraicPoint("h1")] = PieceFactory.White(PieceType.Bishop),
                [new AlgebraicPoint("j1")] = PieceFactory.White(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_white_kingside_castle()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("h1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
                [new AlgebraicPoint("j1")] = PieceFactory.White(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_black_kingside_castle()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("h10").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
                [new AlgebraicPoint("j10")] = PieceFactory.Black(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_white_queenside_castle()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("d1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.QueensideCastle,
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
                [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_black_queenside_castle()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("d10").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.QueensideCastle,
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
                [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_white_vertical_castle()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("f3").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.VerticalCastle,
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
                [new AlgebraicPoint("f10")] = PieceFactory.White(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_black_vertical_castle()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("f8").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.VerticalCastle,
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
                [new AlgebraicPoint("f1")] = PieceFactory.Black(PieceType.Rook),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_white_radioactive_beta_decay()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("e6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
            CapturesMask = UInt128.One << new AlgebraicPoint("e6").AsIdx(),
        };
        AssertMoveUndo(
            new() { [new AlgebraicPoint("e6")] = PieceFactory.White(PieceType.Queen) },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_black_radioactive_beta_decay()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("e6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
            CapturesMask = UInt128.One << new AlgebraicPoint("e6").AsIdx(),
        };
        AssertMoveUndo(
            new() { [new AlgebraicPoint("e6")] = PieceFactory.Black(PieceType.Queen) },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_il_vaticano()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("d5").AsIdx(),
            To = new AlgebraicPoint("g5").AsIdx(),
            Piece = new() { Type = PieceType.Bishop, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.IlVaticano,
            CapturesMask =
                (UInt128.One << new AlgebraicPoint("e5").AsIdx())
                | (UInt128.One << new AlgebraicPoint("f5").AsIdx()),
        };
        AssertMoveUndo(
            new()
            {
                [new AlgebraicPoint("d5")] = PieceFactory.White(PieceType.Bishop),
                [new AlgebraicPoint("e5")] = PieceFactory.Black(),
                [new AlgebraicPoint("f5")] = PieceFactory.Black(),
                [new AlgebraicPoint("g5")] = PieceFactory.White(PieceType.Bishop),
            },
            move
        );
    }

    [Fact]
    public void UndoMove_restores_omnipotent_pawn()
    {
        BitMove move = new()
        {
            From = new AlgebraicPoint("h3").AsIdx(),
            To = new AlgebraicPoint("h3").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
            CapturesMask = UInt128.One << new AlgebraicPoint("h3").AsIdx(),
        };
        AssertMoveUndo(new() { [new AlgebraicPoint("h3")] = PieceFactory.Black() }, move);
    }

    [Fact]
    public void UndoMove_restores_en_passant_state()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f6")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e9").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
        };

        MoveUndoState undo = board.MakeMove(move);

        board.EnPassantSquaresMask.Should().NotBe(0);
        board.EnPassantPawnSquare.Should().Be(move.To);

        board.UndoMove(undo);

        board.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void UndoMove_restores_IsWhiteToMove()
    {
        BitBoard board = new();

        MoveUndoState undo = board.MakeMove(default);

        board.IsWhiteToMove.Should().BeFalse();

        board.UndoMove(undo);

        board.IsWhiteToMove.Should().BeTrue();
    }

    [Fact]
    public void UndoMove_restores_LastCaptureMask()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("a2")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("a2").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << new AlgebraicPoint("a2").AsIdx(),
        };

        MoveUndoState undo = board.MakeMove(move);

        board.LastCaptureMask.Should().Be(move.CapturesMask);

        board.UndoMove(undo);

        board.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void UndoMove_restores_stun_state_after_decrement()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a5")] = PieceFactory.White(PieceType.Rook),
        };
        Dictionary<AlgebraicPoint, int> stunned = new() { [new("a5")] = 2 };

        BitMove move = new()
        {
            From = new AlgebraicPoint("a5").AsIdx(),
            To = new AlgebraicPoint("b5").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };

        AssertMoveUndo(pieces, move, stunned);
    }

    [Fact]
    public void UndoMove_restores_stun_state_after_decrement_and_removal()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a5")] = PieceFactory.White(PieceType.Rook),
        };
        Dictionary<AlgebraicPoint, int> stunned = new() { [new("a5")] = 1 };

        BitMove move = new()
        {
            From = new AlgebraicPoint("a5").AsIdx(),
            To = new AlgebraicPoint("a6").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };

        AssertMoveUndo(pieces, move, stunned);
    }

    [Fact]
    public void UndoMove_restores_throw()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.Queen),
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
        };

        BitMove move = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("f7").AsIdx(),
            Piece = new BitPiece() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.Throw,
        };

        AssertMoveUndo(pieces, move);
    }

    [Fact]
    public void UndoMove_restores_throw_with_stun()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.Queen),
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("e9")] = PieceFactory.Black(PieceType.King),
        };

        BitMove move = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("e9").AsIdx(),
            Piece = new BitPiece() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << new AlgebraicPoint("e2").AsIdx(),
            SpecialMoveType = SpecialMoveType.Throw,
        };

        AssertMoveUndo(pieces, move);
    }

    private static void AssertMoveUndo(
        Dictionary<AlgebraicPoint, Piece> pieces,
        BitMove move,
        Dictionary<AlgebraicPoint, int>? stunned = null
    )
    {
        BitBoard board = BitBoard.FromPieces(pieces, stunnedPositions: stunned);
        BitBoard original = BitBoard.FromPieces(pieces, stunnedPositions: stunned);

        MoveUndoState undo = board.MakeMove(move);
        board.UndoMove(undo);

        board.Should().BeEquivalentTo(original);
    }
}
