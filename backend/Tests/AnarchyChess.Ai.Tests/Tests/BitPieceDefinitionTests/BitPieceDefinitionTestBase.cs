using AnarchyChess.EngineShared.Extensions;
using AnarchyChess.EngineTests.Shared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.BitPieceDefinitionTests;

public class BitPieceDefinitionTestBase
{
    private readonly BitMovesGenerator _generator = new();

    protected void TestMoves(PieceTestCase testCase)
    {
        testCase.BlockedBy.Add(testCase.Origin, testCase.Piece);
        BitBoard board = BitBoard.FromPieces(testCase.BlockedBy);

        Span<BitMove> moves = stackalloc BitMove[256];
        int movesCount = 0;

        BitPieceColor color = testCase.Piece.Color.Match(
            whenWhite: BitPieceColor.White,
            whenBlack: BitPieceColor.Black,
            whenNeutral: BitPieceColor.Neutral
        );
        _generator.GenerateForPiece(
            board,
            testCase.Origin.AsIdx(),
            testCase.Piece.Type,
            color,
            moves,
            ref movesCount
        );

        List<BitMove> expectedMoves = [];
        foreach (var move in testCase.ExpectedMoves)
        {
            UInt128 captures = 0;
            foreach (var capture in move.Captures)
            {
                captures |= UInt128.One << capture.Position.AsIdx();
            }

            expectedMoves.Add(
                new BitMove()
                {
                    From = move.From.AsIdx(),
                    To = move.To.AsIdx(),
                    Piece = testCase.Piece.Type,
                    Captures = captures,
                    SpecialMoveType = move.SpecialMoveType,
                }
            );
        }

        List<BitMove> result = [.. moves[..movesCount]];
        result.Should().BeEquivalentTo(expectedMoves);
        movesCount.Should().Be(expectedMoves.Count);
    }
}
