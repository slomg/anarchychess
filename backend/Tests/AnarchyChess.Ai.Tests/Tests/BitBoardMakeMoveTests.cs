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
        int initialBlackMaterial = board.BlackMaterialCount;

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
        board.BlackMaterialCount.Should().Be(initialBlackMaterial);
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

        board.BlackMaterialCount.Should().Be(MaterialValue.GetPieceValue(PieceType.Rook)); // only black rook left

        board.HasMoved.Should().Be(UInt128.One << move.To);
        board.WhitePieces.Should().Be(UInt128.One << move.To);
        board.BlackPieces.Should().Be(UInt128.One << new AlgebraicPoint("a1").AsIdx());

        undo.CaptureCount.Should().Be(3);
    }

    [Fact]
    public void MakeMove_handles_white_promotion_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("b9")] = PieceFactory.White(PieceType.Pawn, hasMoved: true),
            [new AlgebraicPoint("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
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
                    - MaterialValue.GetPieceValue(PieceType.Pawn)
                    + MaterialValue.GetPieceValue(PieceType.Queen)
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
                    - MaterialValue.GetPieceValue(PieceType.Pawn)
                    + MaterialValue.GetPieceValue(PieceType.Queen)
            );
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
    public void MakeMove_handles_queentum_tunneling()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new AlgebraicPoint("e1")] = PieceFactory.White(PieceType.Queen),
            [new AlgebraicPoint("f7")] = PieceFactory.White(PieceType.Antiqueen),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e1").AsIdx(),
            To = new AlgebraicPoint("f7").AsIdx(),
            Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.QueentumTunnel,
        };
        board.MakeMove(move);

        AssertPieceAt(board, new("e1"), PieceType.Antiqueen);
        AssertPieceAt(board, new("f7"), PieceType.Queen);
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

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    [InlineData(PieceType.SterilePawn)]
    public void MakeMove_sets_en_passant_state_correctly_for_pawn_moves(PieceType pawnType)
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new AlgebraicPoint("b2")] = PieceFactory.White(pawnType),
            },
            isWhiteToMove: true
        );

        BitMove move = new()
        {
            Piece = new BitPiece() { Type = pawnType, Color = BitPieceColor.White },
            From = new AlgebraicPoint("b2").AsIdx(),
            To = new AlgebraicPoint("b5").AsIdx(),
        };
        board.MakeMove(move);

        var expectedEnPassantSquare =
            (UInt128.One << new AlgebraicPoint("b3").AsIdx())
            | (UInt128.One << new AlgebraicPoint("b4").AsIdx());
        board.EnPassantSquaresMask.Should().Be(expectedEnPassantSquare);
        board.EnPassantPawnSquare.Should().Be(move.To);
    }

    [Fact]
    public void MakeMove_doesnt_set_en_passant_state_if_move_is_special()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new AlgebraicPoint("b2")] = PieceFactory.White(PieceType.Pawn),
            },
            isWhiteToMove: true
        );

        BitMove move = new()
        {
            Piece = new BitPiece() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            From = new AlgebraicPoint("b2").AsIdx(),
            To = new AlgebraicPoint("b5").AsIdx(),
            SpecialMoveType = SpecialMoveType.Throw,
        };
        board.MakeMove(move);

        board.EnPassantSquaresMask.Should().Be(0);
        board.EnPassantPawnSquare.Should().Be(0);
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    [InlineData(PieceType.SterilePawn)]
    public void MakeMove_sets_en_passant_state_correctly_for_black_pawn(PieceType pawnType)
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new AlgebraicPoint("c9")] = PieceFactory.Black(pawnType),
            },
            isWhiteToMove: false
        );

        BitMove move = new()
        {
            From = new AlgebraicPoint("c9").AsIdx(),
            To = new AlgebraicPoint("c6").AsIdx(),
            Piece = new BitPiece() { Type = pawnType, Color = BitPieceColor.Black },
        };
        board.MakeMove(move);

        var expectedEnPassantSquare =
            (UInt128.One << new AlgebraicPoint("c8").AsIdx())
            | (UInt128.One << new AlgebraicPoint("c7").AsIdx());
        board.EnPassantSquaresMask.Should().Be(expectedEnPassantSquare);
        board.EnPassantPawnSquare.Should().Be(move.To);
    }

    [Fact]
    public void MakeMove_sets_LastCaptureMask()
    {
        BitBoard board = new();

        BitMove move = new()
        {
            From = 1,
            To = 2,
            Piece = default,
            CapturesMask = (UInt128.One << 1) | (UInt128.One << 15),
        };
        board.MakeMove(move);

        board.LastCaptureMask.Should().Be(move.CapturesMask);
    }

    [Fact]
    public void MakeMove_decrements_stun_and_removes_when_zero()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
        };
        Dictionary<AlgebraicPoint, int> stunned = new() { [new("a1")] = 2 };
        BitBoard board = BitBoard.FromPieces(pieces, stunnedPositions: stunned);

        BitMove move1 = new()
        {
            From = new AlgebraicPoint("a1").AsIdx(),
            To = new AlgebraicPoint("b1").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        board.MakeMove(move1);

        board.StunnedPieces.Should().Be(UInt128.One << new AlgebraicPoint("a1").AsIdx());

        BitMove move2 = new()
        {
            From = new AlgebraicPoint("b1").AsIdx(),
            To = new AlgebraicPoint("c1").AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        board.MakeMove(move2);

        board.StunnedPieces.Should().Be(0);
    }

    [Fact]
    public void MakeMove_handles_throw()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.Queen),
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("f7").AsIdx(),
            Piece = new BitPiece() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.Throw,
        };
        board.MakeMove(move);

        board.StunnedPieces.Should().Be(UInt128.One << move.To);
        AssertPieceAtIdx(board, move.To, PieceType.Pawn, BitPieceColor.White);
        AssertNoPieceAtIdx(board, move.From);
        AssertPieceAt(board, new("e1"), PieceType.Queen, BitPieceColor.White);
    }

    [Fact]
    public void MakeMove_handles_throw_with_stuns()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.Queen),
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("e9")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        BitMove move = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("e9").AsIdx(),
            Piece = new BitPiece() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << new AlgebraicPoint("e2").AsIdx(),
            SpecialMoveType = SpecialMoveType.Throw,
        };
        board.MakeMove(move);

        board.StunnedPieces.Should().Be(UInt128.One << move.To);
        AssertPieceAtIdx(board, move.To, PieceType.King, BitPieceColor.Black);
        AssertNoPieceAtIdx(board, move.From);
        AssertPieceAt(board, new("e1"), PieceType.Queen, BitPieceColor.White);
    }

    private static void AssertPieceAt(
        BitBoard board,
        AlgebraicPoint point,
        PieceType type,
        BitPieceColor? color = null
    ) => AssertPieceAtIdx(board, point.AsIdx(), type, color);

    private static void AssertPieceAtIdx(
        BitBoard board,
        byte idx,
        PieceType type,
        BitPieceColor? color = null
    )
    {
        var piece = board.GetPieceAt(idx);
        piece.Should().NotBeNull();
        piece.Value.Type.Should().Be(type);

        if (color is not null)
        {
            piece.Value.Color.Should().Be(color.Value);
        }

        (board.BitboardFor(type, piece.Value.Color) & (UInt128.One << idx)).Should().NotBe(0);
    }

    private static void AssertNoPieceAt(BitBoard board, AlgebraicPoint point) =>
        AssertNoPieceAtIdx(board, point.AsIdx());

    private static void AssertNoPieceAtIdx(BitBoard board, byte idx)
    {
        board.GetPieceAt(idx).Should().BeNull();
        (board.WhitePieces & (UInt128.One << idx)).Should().Be(0);
        (board.BlackPieces & (UInt128.One << idx)).Should().Be(0);
    }
}
