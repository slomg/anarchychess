using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Game;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class ZobristTests
{
    [Fact]
    public void Compute_creates_equivalent_keys_when_creating_the_same_position()
    {
        Dictionary<AlgebraicPoint, int> stunnedPositions = new() { [new("j1")] = 2 };
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("e9").AsIdx(),
            To: new AlgebraicPoint("e6").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: GameLogicConstants.WhiteOmnipotentPawnMask,
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board1 = BitBoard.FromPieces(
            GameConstants.StartingPosition,
            stunnedPositions: stunnedPositions,
            prevMoveState: prevMoveState
        );
        BitBoard board2 = BitBoard.FromPieces(
            GameConstants.StartingPosition,
            stunnedPositions: stunnedPositions,
            prevMoveState: prevMoveState
        );

        Zobrist.Compute(board1).Should().Be(Zobrist.Compute(board2));
    }

    [Fact]
    public void Compute_changes_when_piece_moves_square()
    {
        Dictionary<AlgebraicPoint, Piece> pieces1 = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
        };
        Dictionary<AlgebraicPoint, Piece> pieces2 = new()
        {
            [new("e3")] = PieceFactory.White(PieceType.Pawn),
        };

        BitBoard board1 = BitBoard.FromPieces(pieces1);
        BitBoard board2 = BitBoard.FromPieces(pieces2);

        Zobrist.Compute(board1).Should().NotBe(Zobrist.Compute(board2));
    }

    [Fact]
    public void Compute_changes_when_side_to_move_changes()
    {
        BitBoard board1 = BitBoard.FromPieces(GameConstants.StartingPosition, isWhiteToMove: true);
        BitBoard board2 = BitBoard.FromPieces(GameConstants.StartingPosition, isWhiteToMove: false);

        Zobrist.Compute(board1).Should().NotBe(Zobrist.Compute(board2));
    }

    [Fact]
    public void Compute_changes_when_stunned_state_changes()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
        };

        Dictionary<AlgebraicPoint, int> stunned1 = [];
        Dictionary<AlgebraicPoint, int> stunned2 = new() { [new("e2")] = 2 };

        BitBoard board1 = BitBoard.FromPieces(pieces, stunnedPositions: stunned1);
        BitBoard board2 = BitBoard.FromPieces(pieces, stunnedPositions: stunned2);

        Zobrist.Compute(board1).Should().NotBe(Zobrist.Compute(board2));
    }

    [Fact]
    public void Compute_changes_when_en_passant_changes()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e6")] = PieceFactory.White(PieceType.Pawn),
        };

        PrevMoveState prevMoveState1 = new(
            From: new AlgebraicPoint("e9").AsIdx(),
            To: new AlgebraicPoint("e6").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: 0,
            SpecialMoveType: SpecialMoveType.None
        );
        PrevMoveState prevMoveState2 = new(
            From: new AlgebraicPoint("e7").AsIdx(),
            To: new AlgebraicPoint("e6").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: 0,
            SpecialMoveType: SpecialMoveType.None
        );

        BitBoard board1 = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState1);
        board1.EnPassantPawnSquare.Should().Be(new AlgebraicPoint("e6").AsIdx());
        BitBoard board2 = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState2);
        board2.EnPassantPawnSquare.Should().Be(0);

        Zobrist.Compute(board1).Should().NotBe(Zobrist.Compute(board2));
    }

    [Fact]
    public void Compute_changes_when_can_spawn_omnipotent_pawn_changes()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("h3")] = PieceFactory.Black(PieceType.Queen),
        };

        PrevMoveState prevMoveState1 = new(
            From: new AlgebraicPoint("h4").AsIdx(),
            To: new AlgebraicPoint("h3").AsIdx(),
            Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            CaptureMask: UInt128.One << new AlgebraicPoint("h3").AsIdx(),
            SpecialMoveType: SpecialMoveType.None
        );
        PrevMoveState prevMoveState2 = new(
            From: new AlgebraicPoint("h4").AsIdx(),
            To: new AlgebraicPoint("h3").AsIdx(),
            Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.Black },
            CaptureMask: 0,
            SpecialMoveType: SpecialMoveType.None
        );

        BitBoard board1 = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState1);
        board1.CanSpawnOmnipotentPawn.Should().BeTrue();
        BitBoard board2 = BitBoard.FromPieces(pieces, prevMoveState: prevMoveState2);
        board2.CanSpawnOmnipotentPawn.Should().BeFalse();

        Zobrist.Compute(board1).Should().NotBe(Zobrist.Compute(board2));
    }

    [Fact]
    public void Compute_changes_when_has_moved_changes()
    {
        Dictionary<AlgebraicPoint, Piece> pieces1 = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("e1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
        };
        Dictionary<AlgebraicPoint, Piece> pieces2 = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.King, hasMoved: true),
            [new("e1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
        };

        BitBoard board1 = BitBoard.FromPieces(pieces1);
        BitBoard board2 = BitBoard.FromPieces(pieces2);

        Zobrist.Compute(board1).Should().NotBe(Zobrist.Compute(board2));
    }
}
