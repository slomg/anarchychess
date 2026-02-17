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
    public async Task FindBestMoveAsync_returns_expected_move()
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

        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f6")] = PieceFactory.White(),
            [new("h7")] = PieceFactory.Black(),
        };
        bool isWhiteToMove = true;

        BitBoard expectedBoard = BitBoard.FromPieces(pieces, isWhiteToMove: isWhiteToMove);
        _aiEngineMock
            .FindBestMove(
                ArgEx.FluentAssert<BitBoard>(x => x.Should().BeEquivalentTo(expectedBoard)),
                depth: AiEngineService.Depth
            )
            .Returns(move);

        var response = await _engine.FindBestMoveAsync(
            new(pieces, IsWhiteToMove: isWhiteToMove, PrevMoveState: null)
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
    public async Task FindBestMoveAsync_passes_prev_move_state_correctly()
    {
        AlgebraicPoint from = new("e2");
        AlgebraicPoint to = new("e4");
        BitMove move = new()
        {
            From = from.AsIdx(),
            To = to.AsIdx(),
            Piece = new() { Type = PieceType.Horsey, Color = BitPieceColor.White },
        };

        AlgebraicPoint prevCapture1 = new("b5");
        AlgebraicPoint prevCapture2 = new("a6");
        PrevMoveStateDto prevMoveDto = new(
            From: new("a5"),
            To: new("a2"),
            Piece: PieceFactory.Black(PieceType.Pawn),
            Captures: [prevCapture1, prevCapture2]
        );

        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Horsey),
            [new("d3")] = PieceFactory.Black(PieceType.Pawn),
        };

        _aiEngineMock
            .FindBestMove(
                ArgEx.FluentAssert<BitBoard>(board =>
                {
                    board.Should().NotBeNull();

                    board.EnPassantPawnSquare.Should().Be(prevMoveDto.To.AsIdx());
                    board
                        .EnPassantSquaresMask.Should()
                        .Be(
                            (UInt128.One << new AlgebraicPoint("a3").AsIdx())
                                | (UInt128.One << new AlgebraicPoint("a4").AsIdx())
                        );
                    board
                        .LastCaptureMask.Should()
                        .Be(
                            (UInt128.One << prevCapture1.AsIdx())
                                | (UInt128.One << prevCapture2.AsIdx())
                        );
                }),
                depth: AiEngineService.Depth
            )
            .Returns(move);

        var response = await _engine.FindBestMoveAsync(
            new(pieces, IsWhiteToMove: true, PrevMoveState: prevMoveDto)
        );

        AiEngineMoveReply expectedReply = new(From: from, To: to, Captures: [], PromotesTo: null);
        response.Should().BeEquivalentTo(expectedReply);
    }

    [Fact]
    public async Task FindBestMoveAsync_returns_null_when_no_move_is_found()
    {
        _aiEngineMock
            .FindBestMove(Arg.Any<BitBoard>(), AiEngineService.Depth)
            .Returns((BitMove?)null);

        var response = await _engine.FindBestMoveAsync(
            new(Pieces: [], IsWhiteToMove: true, PrevMoveState: null)
        );

        response.Should().BeNull();
    }
}
