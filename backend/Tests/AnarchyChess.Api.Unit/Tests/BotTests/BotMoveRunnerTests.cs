using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.EngineShared;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.BotTests;

public class BotMoveRunnerTests
{
    private readonly GameToken _testGameToken = "game token 123";

    private readonly BotMoveRunner _runner;

    private readonly IBot _botMock = Substitute.For<IBot>();
    private readonly IGrainFactory _grainsMock = Substitute.For<IGrainFactory>();
    private readonly IBotGrain _botGrainMock = Substitute.For<IBotGrain>();
    private readonly IDelayProvider _delayProviderMock = Substitute.For<IDelayProvider>();

    public BotMoveRunnerTests()
    {
        _grainsMock.GetGrain<IBotGrain>(_testGameToken).Returns(_botGrainMock);

        _runner = new(Substitute.For<ILogger<BotMoveRunner>>(), _grainsMock, _delayProviderMock);
    }

    [Fact]
    public async Task RunMove_finds_and_plays_best_move()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>() { [new("a1")] = PieceFactory.White() }
        );
        int lastEval = 6969;

        var expectedMove = new MoveEvaluationFaker().Generate();
        _botMock
            .FindMoveAsync(board, lastEval: lastEval, CancellationToken.None)
            .Returns(expectedMove);

        _runner.RunMove(board, lastEval: lastEval, _testGameToken, _botMock);

        await Wait.UntilAsync(() => _botMock.ReceivedCalls().Any());

        await _botMock.Received(1).FindMoveAsync(board, lastEval, CancellationToken.None);
        await _delayProviderMock.Received(1).DelayAsync(1000);
        await _botGrainMock
            .Received(1)
            .PlayBotMoveAsync(expectedMove, Arg.Any<CancellationToken>());
    }
}
