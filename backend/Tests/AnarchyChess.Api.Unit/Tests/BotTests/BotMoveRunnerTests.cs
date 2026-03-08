using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.EngineShared;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.BotTests;

public class BotMoveRunnerTests
{
    private readonly GameToken _testGameToken = "game token 123";

    private readonly BotMoveRunner _runner;

    private readonly IBotService _botServiceMock = Substitute.For<IBotService>();
    private readonly IGrainFactory _grainsMock = Substitute.For<IGrainFactory>();
    private readonly IBotGrain _botGrainMock = Substitute.For<IBotGrain>();

    public BotMoveRunnerTests()
    {
        _grainsMock.GetGrain<IBotGrain>(_testGameToken).Returns(_botGrainMock);

        _runner = new(_botServiceMock, _grainsMock);
    }

    [Fact]
    public async Task RunMove_finds_and_plays_best_move()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>() { [new("a1")] = PieceFactory.White() }
        );
        var expectedMove = new AiEngineMoveFaker().Generate();
        _botServiceMock
            .FindBestMoveAsync(board, Arg.Any<CancellationToken>())
            .Returns(expectedMove);

        _runner.RunMove(board, _testGameToken);

        await Wait.UntilAsync(() => _botServiceMock.ReceivedCalls().Any());

        await _botServiceMock.Received(1).FindBestMoveAsync(board, Arg.Any<CancellationToken>());
        await _botGrainMock
            .Received(1)
            .PlayBotMoveAsync(expectedMove, Arg.Any<CancellationToken>());
    }
}
