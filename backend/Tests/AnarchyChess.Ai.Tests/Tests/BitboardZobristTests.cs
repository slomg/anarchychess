using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitboardZobristTests
{
    [Fact]
    public void ZobristKey_is_equivalent_for_the_same_position_reached_different_ways()
    {
        Dictionary<AlgebraicPoint, Piece> startPieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard boardA = BitBoard.FromPieces(startPieces);
        boardA.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e1").AsIdx(),
                To = new AlgebraicPoint("e2").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );
        boardA.MakeNullMove();
        boardA.MakeMove(
            new BitMove
            {
                From = new AlgebraicPoint("e2").AsIdx(),
                To = new AlgebraicPoint("e3").AsIdx(),
                Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            }
        );

        BitBoard boardB = BitBoard.FromPieces(startPieces);
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
    public void ZobristKey_is_equivalent_for_positions_with_same_omnipotent_pawn()
    {
        BitBoard board1 = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("h8")] = PieceFactory.White(PieceType.Queen, hasMoved: true),
            },
            isWhiteToMove: false,
            prevMoveState: new(
                From: new AlgebraicPoint("h7").AsIdx(),
                To: new AlgebraicPoint("h8").AsIdx(),
                Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.White },
                CaptureMask: UInt128.One << new AlgebraicPoint("h8").AsIdx(),
                SpecialMoveType: SpecialMoveType.None
            )
        );

        BitBoard board2 = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("h7")] = PieceFactory.White(PieceType.Queen, hasMoved: true),
                [new("h8")] = PieceFactory.Black(PieceType.Rook, hasMoved: true),
            }
        );
        board2.MakeMove(
            new()
            {
                From = new AlgebraicPoint("h7").AsIdx(),
                To = new AlgebraicPoint("h8").AsIdx(),
                Piece = new() { Type = PieceType.Queen, Color = BitPieceColor.White },
                CapturesMask = UInt128.One << new AlgebraicPoint("h8").AsIdx(),
            }
        );

        Zobrist.Compute(board1).Should().Be(Zobrist.Compute(board2));
    }
}
