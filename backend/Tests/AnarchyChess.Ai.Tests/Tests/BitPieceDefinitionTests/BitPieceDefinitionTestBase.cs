using AnarchyChess.Api.GameLogic.Models;
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

        List<BitMove> expectedMoves = ConvertUiMovesToBitMoves(testCase.ExpectedMoves);
        List<BitMove> result = [.. moves[..moveCount]];

        // for better assertion logs
        var expectedMoveSorted = expectedMoves.OrderBy(x => x.To);
        var resultSorted = result.OrderBy(x => x.To);
        resultSorted.Should().BeEquivalentTo(expectedMoveSorted);
        moveCount.Should().Be(expectedMoves.Count);
    }

    private static List<BitMove> ConvertUiMovesToBitMoves(List<Move> uiMoves)
    {
        List<BitMove> bitMoves = [];
        foreach (var move in uiMoves)
        {
            BitMove bitMove = new()
            {
                From = move.From.AsIdx(),
                To = move.To.AsIdx(),
                Piece = move.Piece.Type,
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

            bitMoves.Add(bitMove);
        }

        return [.. bitMoves.Distinct()];
    }
}
