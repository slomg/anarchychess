using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
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
        Move? lastMove = testCase.PriorMoves.LastOrDefault();
        BitBoard board = BitBoard.FromPieces(
            testCase.BlockedBy,
            isWhiteToMove: testCase.MovingPlayer is GameColor.White,
            prevMove: lastMove is not null ? UiMoveToBitMove(lastMove) : null
        );

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
            bitMoves.Add(UiMoveToBitMove(move));
        }

        return [.. bitMoves.Distinct()];
    }

    private static BitMove UiMoveToBitMove(Move uiMove)
    {
        UInt128 captureMask = 0;
        foreach (var capture in uiMove.Captures)
        {
            captureMask |= UInt128.One << capture.Position.AsIdx();
        }

        BitMove bitMove = new()
        {
            From = uiMove.From.AsIdx(),
            To = uiMove.To.AsIdx(),
            Piece = uiMove.Piece.Type,
            CapturesMask = captureMask,
            ForcedMovePriority = uiMove.ForcedPriority,
            SpecialMoveType = uiMove.SpecialMoveType,
            PromotesTo = uiMove.PromotesTo,
        };

        return bitMove;
    }
}
