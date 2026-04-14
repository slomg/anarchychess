using AnarchyChess.Ai;
using AnarchyChess.Ai.Models;
using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Ai.Service.Services;
using AnarchyChess.Api.Bots.Errors;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;
using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AnarchyChess.Api.Unit.Tests.BotTests;

public class BotServiceTests : BaseUnitTest
{
    private readonly IAiEngineService _aiEngineMock = Substitute.For<IAiEngineService>();
    private readonly BotService _bot;

    public BotServiceTests()
    {
        _bot = new(Substitute.For<ILogger<BotService>>(), _aiEngineMock);
    }

    [Fact]
    public async Task FindBestMoveAsync_passes_correct_request_with_prev_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.Pawn),
        };

        Move lastMove = MoveFaker
            .Capture(
                GameColor.White,
                pieceType: PieceType.Queen,
                captureTypes: [PieceType.Antiqueen, PieceType.Knook]
            )
            .RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle);
        ChessBoard board = new(pieces, moves: [lastMove]);

        var expectedReply = new MoveEvaluationFaker().Generate();
        UInt128 requestCaptures = 0;
        foreach (var capture in lastMove.Captures)
        {
            requestCaptures |= UInt128.One << capture.Position.AsIdx();
        }
        AiEngineMoveRequest expectedRequest = new(
            Pieces: pieces,
            IsWhiteToMove: true,
            PrevMoveState: new(
                From: lastMove.From.AsIdx(),
                To: lastMove.To.AsIdx(),
                Piece: new() { Type = PieceType.Queen, Color = BitPieceColor.White },
                CaptureMask: requestCaptures,
                SpecialMoveType: SpecialMoveType.KingsideCastle
            ),
            Depth: 69,
            StunnedPositions: []
        );
        _aiEngineMock
            .FindBestMoveAsync(
                ArgEx.FluentAssert<AiEngineMoveRequest>(x =>
                    x.Should().BeEquivalentTo(expectedRequest)
                ),
                CT
            )
            .Returns(expectedReply);

        var result = await _bot.FindBestMoveAsync(board, depth: 69, CT);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedReply);
    }

    [Fact]
    public async Task FindBestMoveAsync_passes_null_prev_move_when_no_moves()
    {
        ChessBoard chessBoard = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.White(PieceType.Pawn),
            }
        );

        var expectedReply = new MoveEvaluationFaker().Generate();
        _aiEngineMock
            .FindBestMoveAsync(Arg.Is<AiEngineMoveRequest>(x => x.PrevMoveState == null), CT)
            .Returns(expectedReply);

        var result = await _bot.FindBestMoveAsync(chessBoard, depth: 123, CT);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedReply);
    }

    [Fact]
    public async Task FindBestMoveAsync_returns_BotOffline_when_status_unavailable()
    {
        _aiEngineMock
            .FindBestMoveAsync(Arg.Any<AiEngineMoveRequest>(), CT)
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "unavailable")));

        var result = await _bot.FindBestMoveAsync(new ChessBoard(), depth: 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BotErrors.BotOffline);
    }

    [Fact]
    public async Task FindBestMoveAsync_returns_NoMoveFound_when_status_invalid_argument()
    {
        _aiEngineMock
            .FindBestMoveAsync(Arg.Any<AiEngineMoveRequest>(), CT)
            .ThrowsAsync(
                new RpcException(new Status(StatusCode.InvalidArgument, "invalid argument"))
            );

        var result = await _bot.FindBestMoveAsync(new ChessBoard(), depth: 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BotErrors.NoMoveFound);
    }

    [Fact]
    public async Task FindBestMoveAsync_returns_BotFailure_for_other_exceptions()
    {
        _aiEngineMock
            .FindBestMoveAsync(Arg.Any<AiEngineMoveRequest>(), CT)
            .ThrowsAsync(new RpcException(new Status(StatusCode.Internal, "internal error")));

        var result = await _bot.FindBestMoveAsync(new ChessBoard(), depth: 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BotErrors.BotFailure);
    }

    [Fact]
    public async Task EvaluateAllMovesAsync_returns_moves_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.Pawn),
        };

        ChessBoard board = new(pieces);

        var expectedMoves = new MoveEvaluationFaker().Generate(3);
        var expectedReply = new EvaluateAllMovesReply(Moves: [.. expectedMoves]);

        AiEngineMoveRequest expectedRequest = new(
            Pieces: pieces,
            IsWhiteToMove: true,
            PrevMoveState: null,
            Depth: 16,
            StunnedPositions: []
        );
        _aiEngineMock
            .EvaluateAllMovesAsync(
                ArgEx.FluentAssert<AiEngineMoveRequest>(x =>
                    x.Should().BeEquivalentTo(expectedRequest)
                ),
                CT
            )
            .Returns(expectedReply);

        var result = await _bot.EvaluateAllMovesAsync(board, depth: 16, CT);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedMoves);
    }

    [Fact]
    public async Task EvaluateAllMovesAsync_returns_BotOffline_on_unavailable_status()
    {
        _aiEngineMock
            .EvaluateAllMovesAsync(Arg.Any<AiEngineMoveRequest>(), CT)
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "unavailable")));

        var result = await _bot.EvaluateAllMovesAsync(new ChessBoard(), 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BotErrors.BotOffline);
    }

    [Fact]
    public async Task EvaluateAllMovesAsync_returns_NoMoveFound_on_invalid_argument_status()
    {
        _aiEngineMock
            .EvaluateAllMovesAsync(Arg.Any<AiEngineMoveRequest>(), CT)
            .ThrowsAsync(
                new RpcException(new Status(StatusCode.InvalidArgument, "invalid argument"))
            );

        var result = await _bot.EvaluateAllMovesAsync(new ChessBoard(), 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BotErrors.NoMoveFound);
    }

    [Fact]
    public async Task EvaluateAllMovesAsync_returns_BotFailure_for_other_rpc_exceptions()
    {
        _aiEngineMock
            .EvaluateAllMovesAsync(Arg.Any<AiEngineMoveRequest>(), CT)
            .ThrowsAsync(new RpcException(new Status(StatusCode.Internal, "internal error")));

        var result = await _bot.EvaluateAllMovesAsync(new ChessBoard(), 123, CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BotErrors.BotFailure);
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
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "unavailable")));

        var result = await _bot.CheckHealthAsync(CT);

        result.Should().BeFalse();
    }

    [Fact]
    public void ConvertBoardToBit_creates_the_correct_bitboard()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.Horsey),
        };
        ChessBoard board = new(pieces);

        BitBoard result = _bot.ConvertBoardToBit(board);

        var expected = BitBoard.FromPieces(pieces, isWhiteToMove: true, prevMoveState: null);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ConvertBoardToBit_removes_stunned_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.Horsey),
            [new("a1")] = PieceFactory.White(PieceType.Queen),
        };
        Dictionary<AlgebraicPoint, int> stunned = new() { [new("d7")] = 2 };

        ChessBoard board = new(pieces, stunnedPieces: stunned);

        BitBoard result = _bot.ConvertBoardToBit(board);

        var expectedPieces = pieces.Where(p => !stunned.ContainsKey(p.Key)).ToDictionary();

        var expected = BitBoard.FromPieces(
            expectedPieces,
            isWhiteToMove: true,
            prevMoveState: null
        );

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    [InlineData(null)]
    public void ConvertBoardToBit_returns_correct_bitboard_with_prev_move(GameColor? lastColor)
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("d7")] = PieceFactory.Black(PieceType.Horsey),
        };
        Move lastMove = new MoveFaker()
            .RuleFor(x => x.Piece, new Piece(PieceType.Pawn, lastColor))
            .RuleFor(
                x => x.Captures,
                [new MoveCapture(new Piece(PieceType.Pawn, GameColor.White), new("a1"))]
            );

        ChessBoard board = new(
            pieces,
            moves: [lastMove],
            sideToMove: lastColor is GameColor.White ? GameColor.Black : GameColor.White
        );

        PrevMoveState expectedPrevMove = new(
            From: lastMove.From.AsIdx(),
            To: lastMove.To.AsIdx(),
            Piece: new()
            {
                Type = lastMove.Piece.Type,
                Color = lastColor.Match(
                    whenWhite: BitPieceColor.White,
                    whenBlack: BitPieceColor.Black,
                    whenNeutral: BitPieceColor.Neutral
                ),
            },
            CaptureMask: UInt128.One << new AlgebraicPoint("a1").AsIdx(),
            SpecialMoveType: SpecialMoveType.None
        );

        BitBoard result = _bot.ConvertBoardToBit(board);

        var expected = BitBoard.FromPieces(
            pieces,
            isWhiteToMove: board.SideToMove is GameColor.White,
            prevMoveState: expectedPrevMove
        );

        result.Should().BeEquivalentTo(expected);
    }
}
