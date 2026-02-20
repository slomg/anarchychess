using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.AnarchyBot.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AnarchyChess.Api.Unit.Tests.AnarchyBotTests;

public class AnarchyBotServiceTests : BaseUnitTest
{
    private readonly IAiEngineService _aiEngineMock = Substitute.For<IAiEngineService>();
    private readonly AnarchyBotService _bot;

    public AnarchyBotServiceTests()
    {
        _bot = new(Substitute.For<ILogger<AnarchyBotService>>(), _aiEngineMock);
    }

    [Fact]
    public async Task FindBestMoveAsync_passes_correct_request_with_prev_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.Pawn),
        };

        Move lastMove = MoveFaker.Capture(GameColor.White, [PieceType.Antiqueen, PieceType.Knook]);
        ChessBoard board = new(pieces, moves: [lastMove]);

        var expectedReply = new AiEngineMoveReplyFaker().Generate();
        AiEngineMoveRequest expectedRequest = new(
            Pieces: pieces,
            IsWhiteToMove: true,
            PrevMoveState: new(
                From: lastMove.From,
                To: lastMove.To,
                Piece: lastMove.Piece,
                Captures: [.. lastMove.Captures.Select(x => x.Position)]
            )
        );
        _aiEngineMock
            .FindBestMoveAsync(
                ArgEx.FluentAssert<AiEngineMoveRequest>(x =>
                    x.Should().BeEquivalentTo(expectedRequest)
                ),
                CT
            )
            .Returns(expectedReply);

        var reply = await _bot.FindBestMoveAsync(board, CT);

        reply.Should().Be(expectedReply);
    }

    [Fact]
    public async Task FindBestMoveAsync_passes_null_prev_move_when_no_moves()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
        };
        ChessBoard chessBoard = new(pieces);

        var expectedReply = new AiEngineMoveReplyFaker().Generate();
        _aiEngineMock
            .FindBestMoveAsync(Arg.Is<AiEngineMoveRequest>(x => x.PrevMoveState == null), CT)
            .Returns(expectedReply);

        var reply = await _bot.FindBestMoveAsync(chessBoard, CT);

        reply.Should().Be(expectedReply);
    }

    [Fact]
    public async Task CheckHealthAsync_returns_true_when_ai_engine_is_healthy()
    {
        _aiEngineMock.CheckHealthAsync(CT).Returns(new HealthReply(IsHealthy: true));

        var result = await _bot.CheckHealthAsync(CT);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckHealthAsync_returns_false_when_ai_engine_is_unhealthy()
    {
        _aiEngineMock.CheckHealthAsync(CT).Returns(new HealthReply(IsHealthy: false));

        var result = await _bot.CheckHealthAsync(CT);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckHealthAsync_returns_false_when_ai_engine_throws_exception()
    {
        _aiEngineMock
            .CheckHealthAsync(CT)
            .Throws(new RpcException(new Status(StatusCode.Unavailable, "unavailable")));

        var result = await _bot.CheckHealthAsync(CT);

        result.Should().BeFalse();
    }
}
