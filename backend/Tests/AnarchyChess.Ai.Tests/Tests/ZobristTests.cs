using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Game;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class ZobristTests
{
    [Fact]
    public void ZobristKey_is_equivalent_when_creating_the_same_position()
    {
        Dictionary<AlgebraicPoint, int> stunnedPositions = new() { [new("j1")] = 2 };
        PrevMoveState prevMoveState = new(
            From: new AlgebraicPoint("e9").AsIdx(),
            To: new AlgebraicPoint("e6").AsIdx(),
            Piece: new() { Type = PieceType.Pawn, Color = BitPieceColor.Black },
            CaptureMask: UInt128.One << new AlgebraicPoint("e6").AsIdx(),
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
}
