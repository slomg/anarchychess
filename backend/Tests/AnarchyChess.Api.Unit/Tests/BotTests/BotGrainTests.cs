using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AnarchyChess.EngineShared.Extensions;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.BotTests;

public class BotGrainTests : BaseGrainTest
{
    private readonly GameToken _gameToken = "testtoken";

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();
    private readonly FenNotation _initialFenNotation = new FenNotationFaker().Generate();

    private readonly IBot _anarchyBot = Substitute.For<IBot>();
    private readonly IBot _lobotomizedAnarchyBot = Substitute.For<IBot>();

    private readonly IGameCore _coreMock = Substitute.For<IGameCore>();
    private readonly IBotMoveRunner _botMoveRunnerMock = Substitute.For<IBotMoveRunner>();

    private readonly BotGrainState _state;

    public BotGrainTests()
    {
        _coreMock.StartGame(Arg.Any<GameCoreState>()).Returns(_initialFenNotation);

        _anarchyBot.Type.Returns(BotType.AnarchyBot);
        _lobotomizedAnarchyBot.Type.Returns(BotType.LobotomizedAnarchyBot);

        Silo.ServiceProvider.AddService(_coreMock);
        Silo.ServiceProvider.AddService(_botMoveRunnerMock);
        Silo.ServiceProvider.AddService<IEnumerable<IBot>>([_anarchyBot, _lobotomizedAnarchyBot]);

        _state = Silo.StorageManager.GetStorage<BotGrainState>(BotGrain.StateName).State;
    }

    [Theory]
    [InlineData(GameColor.White, BotType.AnarchyBot)]
    [InlineData(GameColor.White, BotType.LobotomizedAnarchyBot)]
    [InlineData(GameColor.Black, BotType.AnarchyBot)]
    [InlineData(GameColor.Black, BotType.LobotomizedAnarchyBot)]
    public async Task StartGameAsync_creates_correct_state(GameColor playerColor, BotType botType)
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);

        GameColor botColor = playerColor.Invert();
        GamePlayer botPlayer = new GamePlayerFaker(botColor).Generate();
        IBot bot = botType is BotType.AnarchyBot ? _anarchyBot : _lobotomizedAnarchyBot;
        bot.CreateBotPlayer(botColor).Returns(botPlayer);

        ChessBoard board = new(GameConstants.StartingPosition);
        _coreMock.GetReadOnlyBoard(Arg.Any<GameCoreState>()).Returns(board);

        LegalMoveSet legalMoves = new LegalMoveSetFaker().Generate();
        _coreMock.GetLegalMoves(Arg.Any<GameCoreState>()).Returns(legalMoves);

        GamePlayer player = playerColor is GameColor.White ? _whitePlayer : _blackPlayer;
        await grain.StartGameAsync(player, botType, CT);

        _state.CurrentGame.Should().NotBeNull();

        PlayerRoster expectedPlayers = new(
            WhitePlayer: playerColor is GameColor.White ? player : botPlayer,
            BlackPlayer: playerColor is GameColor.Black ? player : botPlayer
        );

        _state
            .CurrentGame.Should()
            .BeEquivalentTo(
                new BotGameData()
                {
                    Players = expectedPlayers,
                    BotColor = botPlayer.Color,
                    HumanColor = playerColor,
                    BotType = botType,
                    InitialFen = _initialFenNotation.FullFen,
                    Core = _state.CurrentGame.Core,
                },
                options => options.Excluding(x => x.MoveHistory)
            );

        if (playerColor is GameColor.Black)
        {
            _botMoveRunnerMock.Received(1).RunMove(board, lastEval: 0, legalMoves, _gameToken, bot);
        }
    }
}
