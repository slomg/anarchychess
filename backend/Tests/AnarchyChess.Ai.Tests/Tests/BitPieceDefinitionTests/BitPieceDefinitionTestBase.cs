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
        int moveCount = 0;

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
            ref moveCount
        );

        List<BitMove> expectedMoves = [];
        foreach (var move in testCase.ExpectedMoves)
        {
            BitMove bitMove = new()
            {
                From = move.From.AsIdx(),
                To = move.To.AsIdx(),
                Piece = testCase.Piece.Type,
                SpecialMoveType = move.SpecialMoveType,
            };
            foreach (var capture in move.Captures)
            {
                BitPieceColor capturedColor = capture.CapturedPiece.Color.Match(
                    whenWhite: BitPieceColor.White,
                    whenBlack: BitPieceColor.Black,
                    whenNeutral: BitPieceColor.Neutral
                );
                bitMove.AddCapture(
                    capture.Position.AsIdx(),
                    capture.CapturedPiece.Type,
                    capturedColor
                );
            }

            expectedMoves.Add(bitMove);
        }

        List<BitMove> result = [.. moves[..moveCount]];
        result.Should().BeEquivalentTo(expectedMoves, options => options.WithoutStrictOrdering());
        moveCount.Should().Be(expectedMoves.Count);
    }
}
