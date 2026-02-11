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
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("a2").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_captures()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d1")] = PieceFactory.White(PieceType.Queen),
            [new("d10")] = PieceFactory.Black(),
            [new("d9")] = PieceFactory.Black(),
            [new("e8")] = PieceFactory.Black(),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

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

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_promotion()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("b9")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("b9").AsIdx(),
            To = new AlgebraicPoint("b10").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_white_kingside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("j1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("h1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_black_kingside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("j10")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("h10").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_white_queenside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("d1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.QueensideCastle,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_black_queenside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("d10").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.QueensideCastle,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_white_vertical_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("f10")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("f3").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.VerticalCastle,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_black_vertical_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("f1")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("f8").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.VerticalCastle,
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_white_radioactive_beta_decay()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e6")] = PieceFactory.White(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
            CapturesMask = UInt128.One << new AlgebraicPoint("e6").AsIdx(),
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_black_radioactive_beta_decay()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e6")] = PieceFactory.Black(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
            CapturesMask = UInt128.One << new AlgebraicPoint("e6").AsIdx(),
        };

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_il_vaticano()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("d5")] = PieceFactory.White(PieceType.Bishop),
            [new AlgebraicPoint("e5")] = PieceFactory.Black(),
            [new AlgebraicPoint("f5")] = PieceFactory.Black(),
            [new AlgebraicPoint("g5")] = PieceFactory.White(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

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

        AssertMoveUndo(board, original, move);
    }

    [Fact]
    public void UndoMove_restores_omnipotent_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("h3")] = PieceFactory.Black(),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        BitBoard original = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("h3").AsIdx(),
            To = new AlgebraicPoint("h3").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
            CapturesMask = UInt128.One << new AlgebraicPoint("h3").AsIdx(),
        };

        AssertMoveUndo(board, original, move);
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

    private static void AssertMoveUndo(BitBoard board, BitBoard original, BitMove move)
    {
        MoveUndoState undo = board.MakeMove(move);
        board.UndoMove(undo);

        board.Should().BeEquivalentTo(original);
    }
}
