using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitBoardMakeMoveTests
{
    [Fact]
    public void MakeMove_moves_piece_to_empty_square_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int initialWhiteMaterial = board.WhiteMaterialCount;
        int initialWhiteKingCount = board.WhiteKingCount;
        int initialBlackMaterial = board.BlackMaterialCount;
        int initialBlackKingCount = board.BlackKingCount;

        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("a2").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        board.MakeMove(move);

        board.GetPieceAt(new AlgebraicPoint("a1").AsIdx()).Should().BeNull();

        AssertPieceAtIdx(board, move.To, PieceType.Rook, BitPieceColor.White);
        board
            .BitboardFor(PieceType.Rook, BitPieceColor.White)
            .Should()
            .Be(UInt128.One << new AlgebraicPoint("a2").AsIdx());

        board.HasMoved.Should().Be(UInt128.One << move.To);
        board.WhitePieces.Should().Be(UInt128.One << move.To);

        board.IsWhiteToMove.Should().BeFalse();

        board.WhiteMaterialCount.Should().Be(initialWhiteMaterial);
        board.WhiteKingCount.Should().Be(initialWhiteKingCount);
        board.BlackMaterialCount.Should().Be(initialBlackMaterial);
        board.BlackKingCount.Should().Be(initialBlackKingCount);
    }

    [Fact]
    public void MakeMove_flips_IsWhiteToMove()
    {
        BitBoard board = new();

        board.IsWhiteToMove.Should().BeTrue();
        board.MakeMove(default);

        board.IsWhiteToMove.Should().BeFalse();
        board.MakeMove(default);

        board.IsWhiteToMove.Should().BeTrue();
    }

    [Fact]
    public void MakeMove_capture_piece_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d1")] = PieceFactory.White(PieceType.Queen, hasMoved: false),
            [new("d10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("d1").AsIdx(),
            To = new AlgebraicPoint("d10").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << new AlgebraicPoint("d10").AsIdx(),
        };
        var undo = board.MakeMove(move);

        AssertPieceAtIdx(board, move.To, PieceType.Queen, BitPieceColor.White);

        board.HasMoved.Should().Be(UInt128.One << move.To);
        board.WhitePieces.Should().Be(UInt128.One << move.To);
        board.BlackPieces.Should().Be(0);

        board.BlackMaterialCount.Should().Be(0);

        undo.CaptureCount.Should().Be(1);
        undo.GetCapture(0).Should().Be((move.To, PieceType.Rook, BitPieceColor.Black));
    }

    [Fact]
    public void MakeMove_handles_multiple_captures()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d1")] = PieceFactory.White(PieceType.Queen, hasMoved: false),
            [new("d10")] = PieceFactory.Black(hasMoved: true),
            [new("d9")] = PieceFactory.Black(hasMoved: true),
            [new("e8")] = PieceFactory.Black(hasMoved: false),
            [new("a1")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("d1").AsIdx(),
            To = new AlgebraicPoint("d9").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask =
                (UInt128.One << new AlgebraicPoint("d10").AsIdx())
                | (UInt128.One << new AlgebraicPoint("d9").AsIdx())
                | UInt128.One << new AlgebraicPoint("e8").AsIdx(),
        };
        var undo = board.MakeMove(move);

        AssertPieceAtIdx(board, move.To, PieceType.Queen, BitPieceColor.White);
        AssertNoPieceAt(board, new("d10"));
        AssertNoPieceAt(board, new("e8"));

        board.BlackMaterialCount.Should().Be(Evaluator.GetPieceValue(PieceType.Rook)); // only black rook left

        board.HasMoved.Should().Be(UInt128.One << move.To);
        board.WhitePieces.Should().Be(UInt128.One << move.To);
        board.BlackPieces.Should().Be(UInt128.One << new AlgebraicPoint("a1").AsIdx());

        undo.CaptureCount.Should().Be(3);
    }

    [Fact]
    public void MakeMove_decrements_white_king_count_on_king_capture()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.Black(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        board.WhiteKingCount.Should().Be(1);

        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("b1").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.Black },
            CapturesMask = UInt128.One << new AlgebraicPoint("b1").AsIdx(),
        };
        board.MakeMove(move);

        board.WhiteKingCount.Should().Be(0);
    }

    [Fact]
    public void MakeMove_decrements_black_king_count_on_king_capture()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        board.BlackKingCount.Should().Be(1);

        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("b1").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.Black },
            CapturesMask = UInt128.One << new AlgebraicPoint("b1").AsIdx(),
        };
        board.MakeMove(move);

        board.BlackKingCount.Should().Be(0);
    }

    [Fact]
    public void MakeMove_handles_white_promotion_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b9")] = PieceFactory.White(PieceType.Pawn, hasMoved: true),
            [new AlgebraicPoint("a1")] = PieceFactory.White(hasMoved: true),
        };
        BitBoard board = BitBoard.FromPieces(pieces);
        int initialWhiteMaterial = board.WhiteMaterialCount;

        BitMove move = new()
        {
            From = new AlgebraicPoint("b9").AsIdx(),
            To = new AlgebraicPoint("b10").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("b10"), PieceType.Queen, BitPieceColor.White);
        board
            .WhiteMaterialCount.Should()
            .Be(
                initialWhiteMaterial
                    - Evaluator.GetPieceValue(PieceType.Pawn)
                    + Evaluator.GetPieceValue(PieceType.Queen)
            );

        board.BitboardFor(PieceType.Pawn, BitPieceColor.White).Should().Be(0);
        board.HasMoved.Should().Be(UInt128.One << new AlgebraicPoint("a1").AsIdx());
    }

    [Fact]
    public void MakeMove_handles_black_promotion_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b2")] = PieceFactory.Black(PieceType.Pawn),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);
        int initialBlackMaterial = board.BlackMaterialCount;

        BitMove move = new()
        {
            From = new AlgebraicPoint("b2").AsIdx(),
            To = new AlgebraicPoint("b1").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            PromotesTo = PieceType.Queen,
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("b1"), PieceType.Queen, BitPieceColor.Black);
        board
            .BlackMaterialCount.Should()
            .Be(
                initialBlackMaterial
                    - Evaluator.GetPieceValue(PieceType.Pawn)
                    + Evaluator.GetPieceValue(PieceType.Queen)
            );
    }

    [Fact]
    public void MakeMove_increments_king_count_for_white_checker_promotion()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b9")] = PieceFactory.White(PieceType.Checker),
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("b9").AsIdx(),
            To = new AlgebraicPoint("c10").AsIdx(),
            Piece = new() { Type = PieceType.Checker, Color = BitPieceColor.White },
            PromotesTo = PieceType.King,
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("c10"), PieceType.King);
        board.WhiteKingCount.Should().Be(2);
    }

    [Fact]
    public void MakeMove_increments_king_count_for_black_checker_promotion()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b2")] = PieceFactory.Black(PieceType.Checker),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("b2").AsIdx(),
            To = new AlgebraicPoint("c1").AsIdx(),
            Piece = new() { Type = PieceType.Checker, Color = BitPieceColor.Black },
            PromotesTo = PieceType.King,
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("c1"), PieceType.King);
        board.BlackKingCount.Should().Be(2);
    }

    [Fact]
    public void MakeMove_handles_white_kingside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("j1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("h1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("j1"));
        AssertPieceAt(board, new("g1"), PieceType.Rook);
    }

    [Fact]
    public void MakeMove_handles_black_kingside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("j10")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("h10").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("j10"));
        AssertPieceAt(board, new("g10"), PieceType.Rook);
    }

    [Fact]
    public void MakeMove_handles_white_queenside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("d1").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.QueensideCastle,
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("a1"));
        AssertPieceAt(board, new("e1"), PieceType.Rook);
    }

    [Fact]
    public void MakeMove_handles_black_queenside_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("a10")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("d10").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.QueensideCastle,
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("a10"));
        AssertPieceAt(board, new("e10"), PieceType.Rook);
    }

    [Fact]
    public void MakeMove_handles_white_vertical_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f1")] = PieceFactory.White(PieceType.King),
            [new AlgebraicPoint("f10")] = PieceFactory.White(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f1").AsIdx(),
            To = new AlgebraicPoint("f3").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.VerticalCastle,
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("f10"));
        AssertPieceAt(board, new("f2"), PieceType.Rook);
    }

    [Fact]
    public void MakeMove_handles_black_vertical_castle()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("f10")] = PieceFactory.Black(PieceType.King),
            [new AlgebraicPoint("f1")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("f10").AsIdx(),
            To = new AlgebraicPoint("f8").AsIdx(),
            Piece = new() { Type = PieceType.King, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.VerticalCastle,
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("f1"));
        AssertPieceAt(board, new("f9"), PieceType.Rook);
    }

    [Fact]
    public void MakeMove_handles_il_vaticano()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("d6")] = PieceFactory.White(PieceType.Bishop),
            [new AlgebraicPoint("e6")] = PieceFactory.Black(),
            [new AlgebraicPoint("f6")] = PieceFactory.Black(),
            [new AlgebraicPoint("g6")] = PieceFactory.White(PieceType.Bishop),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("d6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Bishop, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.IlVaticano,
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("d6"), PieceType.Bishop);
        AssertPieceAt(board, new("g6"), PieceType.Bishop);
    }

    [Fact]
    public void MakeMove_handles_white_radioactive_beta_decay()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e6")] = PieceFactory.White(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
            CapturesMask = UInt128.One << new AlgebraicPoint("e6").AsIdx(),
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("e6"));
        AssertPieceAt(board, new("d6"), PieceType.Rook);
        AssertPieceAt(board, new("f6"), PieceType.Horsey);
        AssertPieceAt(board, new("e7"), PieceType.SterilePawn);
    }

    [Fact]
    public void MakeMove_handles_black_radioactive_beta_decay()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e6")] = PieceFactory.Black(PieceType.Queen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e6").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            SpecialMoveType = SpecialMoveType.RadioactiveBetaDecay,
            CapturesMask = UInt128.One << new AlgebraicPoint("e6").AsIdx(),
        };
        board.MakeMove(move);

        AssertNoPieceAt(board, new("e6"));
        AssertPieceAt(board, new("d6"), PieceType.Rook);
        AssertPieceAt(board, new("f6"), PieceType.Horsey);
        AssertPieceAt(board, new("e5"), PieceType.SterilePawn);
    }

    [Fact]
    public void MakeMove_handles_omnipotent_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("h3")] = PieceFactory.Black(),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("h3").AsIdx(),
            To = new AlgebraicPoint("h3").AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
            CapturesMask = UInt128.One << new AlgebraicPoint("h3").AsIdx(),
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("h3"), PieceType.Pawn, BitPieceColor.White);
    }

    [Fact]
    public void MakeMove_handles_knooklear_fusion()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook),
            [new AlgebraicPoint("c1")] = PieceFactory.White(PieceType.Horsey),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("c1").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KnooklearFusion,
            PromotesTo = PieceType.Knook,
            CapturesMask = UInt128.One << new AlgebraicPoint("c1").AsIdx(),
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("c1"), PieceType.Knook, BitPieceColor.White);
        AssertNoPieceAt(board, new("a1"));
    }

    private static void AssertPieceAt(
        BitBoard board,
        AlgebraicPoint point,
        PieceType type,
        BitPieceColor? color = null
    ) => AssertPieceAtIdx(board, point.AsIdx(), type, color);

    private static void AssertPieceAtIdx(
        BitBoard board,
        byte point,
        PieceType type,
        BitPieceColor? color = null
    )
    {
        var piece = board.GetPieceAt(point);
        piece.Should().NotBeNull();
        piece.Value.Type.Should().Be(type);

        if (color is not null)
        {
            piece.Value.Color.Should().Be(color.Value);
        }
    }

    private static void AssertNoPieceAt(BitBoard board, AlgebraicPoint point) =>
        board.GetPieceAt(point.AsIdx()).Should().BeNull();
}
