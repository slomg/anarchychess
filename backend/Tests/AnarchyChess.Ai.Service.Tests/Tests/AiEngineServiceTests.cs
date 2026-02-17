using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Ai.Service.Tests.Tests;

public class AiEngineServiceTests
{
    private readonly IAiEngine _aiEngineMock = Substitute.For<IAiEngine>();

    private readonly AiEngineService _engine;

    public AiEngineServiceTests()
    {
        _engine = new(_aiEngineMock);
    }

    [Fact]
    public async Task PlayMoveAsync_returns_expected_move()
    {
        AlgebraicPoint from = new("a5");
        AlgebraicPoint to = new("d7");
        AlgebraicPoint capture1 = new("c6");
        AlgebraicPoint capture2 = new("j4");
        PieceType promotesTo = PieceType.Rook;
        BitMove move = new()
        {
            From = from.AsIdx(),
            To = to.AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
            CapturesMask = (UInt128.One << capture1.AsIdx()) | (UInt128.One << capture2.AsIdx()),
            PromotesTo = promotesTo,
        };

        LastMoveState lastMoveState = new(
            EnPassantPawnSquare: 10,
            EnPassantSquaresMask: UInt128.One << 11,
            LastCaptureMask: UInt128.One << 12
        );
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f6")] = PieceFactory.White(),
            [new("h7")] = PieceFactory.Black(),
        };
        bool isWhiteToMove = true;

        BitBoard expectedBoard = BitBoard.FromPieces(
            pieces,
            isWhiteToMove: isWhiteToMove,
            lastMoveState: lastMoveState
        );
        _aiEngineMock
            .FindBestMove(
                ArgEx.FluentAssert<BitBoard>(x => x.Should().BeEquivalentTo(expectedBoard)),
                depth: AiEngineService.Depth
            )
            .Returns(move);

        var response = await _engine.PlayMoveAsync(
            new(pieces, IsWhiteToMove: isWhiteToMove, LastMoveState: lastMoveState)
        );

        AiEngineMoveReply expectedReply = new(
            From: from,
            To: to,
            Captures: [capture1, capture2],
            PromotesTo: promotesTo
        );
        response.Should().BeEquivalentTo(expectedReply);
    }

    [Fact]
    public async Task PlayMoveAsync_returns_null_when_no_move_is_found()
    {
        _aiEngineMock
            .FindBestMove(Arg.Any<BitBoard>(), AiEngineService.Depth)
            .Returns((BitMove?)null);

        var response = await _engine.PlayMoveAsync(
            new(Pieces: [], IsWhiteToMove: true, LastMoveState: null)
        );

        response.Should().BeNull();
    }
}
