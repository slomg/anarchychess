using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Grpc.Core;
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
            .Returns((BestMove: move, EvalForBot: 6969));

        var response = await _engine.FindBestMoveAsync(
            new(pieces, IsWhiteToMove: isWhiteToMove, PrevMoveState: null),
            TestContext.Current.CancellationToken
        );

        AiEngineMove expectedReply = new(
            From: from,
            To: to,
            Captures: [capture1, capture2],
            PromotesTo: promotesTo,
            EvalForBot: 6969
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
            .Returns((BestMove: move, EvalForBot: -6969));

        var response = await _engine.FindBestMoveAsync(
            new(pieces, IsWhiteToMove: true, PrevMoveState: prevMoveDto),
            TestContext.Current.CancellationToken
        );

        AiEngineMove expectedReply = new(
            From: from,
            To: to,
            Captures: [],
            PromotesTo: null,
            EvalForBot: -6969
        );
        response.Should().BeEquivalentTo(expectedReply);
    }

    [Fact]
    public async Task FindBestMoveAsync_throws_when_no_move_is_found()
    {
        _aiEngineMock
            .FindBestMove(Arg.Any<BitBoard>(), AiEngineService.Depth)
            .Returns((BestMove: null, EvalForBot: 0));

        Func<Task> act = async () =>
            await _engine
                .FindBestMoveAsync(new(Pieces: [], IsWhiteToMove: true, PrevMoveState: null))
                .AsTask();

        var ex = await act.Should().ThrowAsync<RpcException>();

        ex.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        ex.Which.Status.Detail.Should()
            .Be("The provided position contains no legal moves and is invalid");
    }

    [Fact]
    public async Task EvaluateAllMovesAsync_returns_expected_moves()
    {
        AlgebraicPoint from1 = new("a1");
        AlgebraicPoint to1 = new("b2");
        AlgebraicPoint capture1 = new("c3");
        BitMove move1 = new()
        {
            From = from1.AsIdx(),
            To = to1.AsIdx(),
            Piece = new() { Type = PieceType.Rook, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << capture1.AsIdx(),
        };

        AlgebraicPoint from2 = new("d4");
        AlgebraicPoint to2 = new("e5");
        BitMove move2 = new()
        {
            From = from2.AsIdx(),
            To = to2.AsIdx(),
            Piece = new() { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CapturesMask = 0,
            PromotesTo = PieceType.Queen,
        };

        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(),
            [new("d4")] = PieceFactory.White(PieceType.Pawn),
        };

        BitBoard expectedBoard = BitBoard.FromPieces(pieces);

        MoveEvaluation[] engineMoves = [new(move1, 100), new(move2, -50)];
        _aiEngineMock
            .EvaluateAllMoves(
                ArgEx.FluentAssert<BitBoard>(x => x.Should().BeEquivalentTo(expectedBoard)),
                depth: AiEngineService.Depth
            )
            .Returns(engineMoves);

        var response = await _engine.EvaluateAllMovesAsync(
            new(pieces, IsWhiteToMove: true, PrevMoveState: null),
            TestContext.Current.CancellationToken
        );

        List<AiEngineMove> expected =
        [
            new(From: from1, To: to1, Captures: [capture1], PromotesTo: null, EvalForBot: 100),
            new(From: from2, To: to2, Captures: [], PromotesTo: PieceType.Queen, EvalForBot: -50),
        ];

        response.Moves.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task EvaluateAllMovesAsync_throws_when_no_moves_are_found()
    {
        _aiEngineMock.EvaluateAllMoves(Arg.Any<BitBoard>(), AiEngineService.Depth).Returns([]);

        Func<Task> act = async () =>
            await _engine
                .EvaluateAllMovesAsync(
                    new(Pieces: [], IsWhiteToMove: true, PrevMoveState: null),
                    TestContext.Current.CancellationToken
                )
                .AsTask();

        var ex = await act.Should().ThrowAsync<RpcException>();

        ex.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        ex.Which.Status.Detail.Should()
            .Be("The provided position contains no legal moves and is invalid");
    }

    [Fact]
    public async Task CheckHealthAsync_returns_true()
    {
        var result = await _engine.CheckHealthAsync(TestContext.Current.CancellationToken);
        result.IsHealthy.Should().BeTrue();
    }
}
