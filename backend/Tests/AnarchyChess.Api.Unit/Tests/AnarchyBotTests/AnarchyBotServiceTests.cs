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
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.AnarchyBotTests;

public class AnarchyBotServiceTests
{
    private readonly IAiEngineService _aiEngineMock = Substitute.For<IAiEngineService>();
    private readonly AnarchyBotService _bot;

    public AnarchyBotServiceTests()
    {
        _bot = new(_aiEngineMock);
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
                )
            )
            .Returns(expectedReply);

        var reply = await _bot.FindBestMoveAsync(board);

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
            .FindBestMoveAsync(Arg.Is<AiEngineMoveRequest>(x => x.PrevMoveState == null))
            .Returns(expectedReply);

        var reply = await _bot.FindBestMoveAsync(chessBoard);

        reply.Should().Be(expectedReply);
    }
}
