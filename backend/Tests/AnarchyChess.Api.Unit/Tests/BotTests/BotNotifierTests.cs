using AnarchyChess.Api.AnarchyBot.Services;
using AnarchyChess.Api.AnarchyBot.SignalR;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.BotTests;

public class BotNotifierTests
{
    private readonly GameToken _gameToken = "game-123";

    private readonly IHubContext<BotHub, IBotHubClient> _hubContextMock = Substitute.For<
        IHubContext<BotHub, IBotHubClient>
    >();
    private readonly IHubClients<IBotHubClient> _clientsMock = Substitute.For<
        IHubClients<IBotHubClient>
    >();
    private readonly IGroupManager _groupsMock = Substitute.For<IGroupManager>();

    private readonly IBotHubClient _clientGroupProxyMock = Substitute.For<IBotHubClient>();

    private readonly BotNotifier _notifier;

    public BotNotifierTests()
    {
        _clientsMock.Group(_gameToken).Returns(_clientGroupProxyMock);

        _hubContextMock.Clients.Returns(_clientsMock);
        _hubContextMock.Groups.Returns(_groupsMock);

        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task NotifyPlayerMadeMoveAsync_sends_new_move()
    {
        var move = new MoveSnapshotFaker().Generate();
        int plyNumber = 523;

        await _notifier.NotifyPlayerMadeMoveAsync(
            _gameToken,
            move,
            plyNumber: plyNumber,
            didMoveEndGame: true
        );

        await _clientGroupProxyMock
            .Received(1)
            .PlayerMadeMoveAsync(move, plyNumber: plyNumber, didMoveEndGame: true);
    }

    [Fact]
    public async Task NotifyBotMadeMoveAsync_sends_new_move_with_legal_moves()
    {
        var move = new MoveSnapshotFaker().Generate();
        CompressedMoves compressedLegalMoves = "test moves";
        int plyNumber = 456;

        await _notifier.NotifyBotMadeMoveAsync(
            _gameToken,
            move,
            plyNumber: plyNumber,
            compressedLegalMoves
        );

        await _clientGroupProxyMock
            .Received(1)
            .BotMadeMoveAsync(move, plyNumber: plyNumber, compressedLegalMoves);
    }

    [Fact]
    public async Task NotifyGameEndedAsync_sends_game_ended()
    {
        var result = new GameResultDataFaker().Generate();

        await _notifier.NotifyGameEndedAsync(_gameToken, result);

        await _clientGroupProxyMock.Received(1).GameEndedAsync(result);
    }
}
