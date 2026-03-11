using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Bots.Bots;
using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.SanNotation;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orleans.TestKit;

namespace AnarchyChess.Api.Integration.Tests.BotTests;

public class BotGrainTests : BaseOrleansIntegrationTest
{
    private readonly GameToken _gameToken = "testtoken";
    private readonly AiEngineMove _firstWhiteBotMove = new(
        From: new("f2"),
        To: new("f5"),
        Captures: [],
        PromotesTo: null,
        EvalForBot: 6969
    );

    private readonly AiEngineMove _firstBlackBotMove = new(
        From: new("f9"),
        To: new("f6"),
        Captures: [],
        PromotesTo: null,
        EvalForBot: 420420
    );

    private readonly IBotNotifier _notifierMock = Substitute.For<IBotNotifier>();
    private readonly IBotService _botServiceMock = Substitute.For<IBotService>();

    private readonly AnarchyBot _anarchyBot;
    private readonly LobotomizedAnarchyBot _lobotomizedAnarchyBot;

    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly ISanCalculator _sanCalculator;
    private readonly IFenEncoder _fenEncoder;
    private readonly IGameCore _core;

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    private readonly GamePlayer _player;

    private readonly BotGrainState _state;

    public BotGrainTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _player = _whitePlayer;

        _core = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();
        _fenEncoder = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IFenEncoder>();
        _sanCalculator = ApiTestBase.Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
        var gameArchiveService =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameArchiveService>();
        var unitOfWork = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        _anarchyBot = new(_botServiceMock);
        _lobotomizedAnarchyBot = new(
            Silo.ServiceProvider.GetRequiredService<ILogger<LobotomizedAnarchyBot>>(),
            _botServiceMock,
            Silo.ServiceProvider.GetRequiredService<IRandomProvider>()
        );
        BotMoveRunner botMoveRunner = new(
            Silo.ServiceProvider.GetRequiredService<ILogger<BotMoveRunner>>(),
            Silo.GrainFactory
        );

        Silo.ServiceProvider.AddService<IEnumerable<IBot>>([_anarchyBot, _lobotomizedAnarchyBot]);
        Silo.ServiceProvider.AddService(_core);
        Silo.ServiceProvider.AddService(_gameResultDescriber);
        Silo.ServiceProvider.AddService(gameArchiveService);
        Silo.ServiceProvider.AddService(unitOfWork);
        Silo.ServiceProvider.AddService<IBotMoveRunner>(botMoveRunner);
        Silo.ServiceProvider.AddService(_notifierMock);

        _state = Silo.StorageManager.GetStorage<BotGrainState>(BotGrain.StateName).State;
    }

    [Fact]
    public async Task SyncPlyNumberAsync_syncs_with_the_correct_ply_number()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain);

        ConnectionId connectionId = "test-connection";

        await grain.PlayMoveAsync(_whitePlayer.UserId, new MoveKey(GetLegalMove()), ApiTestBase.CT);
        await WaitForBotMoveAsync();

        var result = await grain.SyncPlyNumberAsync(connectionId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();

        await _notifierMock.Received(1).SyncPlyNumberAsync(plyNumber: 2, connectionId);
    }

    [Fact]
    public async Task StartGameAsync_plays_bot_move_if_player_color_is_black()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);

        await StartGameAsync(grain, _blackPlayer);

        await AssertBotMoveAsync(_firstWhiteBotMove);
    }

    [Fact]
    public async Task StartGame_doesnt_play_bot_move_if_player_color_is_white()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);

        AiEngineMove botMove = new(
            From: new("f9"),
            To: new("f6"),
            Captures: [],
            PromotesTo: null,
            EvalForBot: 123
        );
        _botServiceMock
            .FindBestMoveAsync(Arg.Any<IReadOnlyChessBoard>(), Arg.Any<int>(), ApiTestBase.CT)
            .Returns(botMove);

        await grain.StartGameAsync(_whitePlayer, botType: BotType.AnarchyBot, ApiTestBase.CT);

        _notifierMock.ReceivedCalls().Should().BeEmpty();
        _state.CurrentGame!.MoveHistory.Moves.Should().BeEmpty();
    }

    [Fact]
    public async Task PlayMoveAsync_notifies_player()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain);

        Move move = GetLegalMove();
        ChessBoard boardCopy = new(_state.CurrentGame!.Core.Board);
        boardCopy.PlayMove(move);
        string expectedFen = _fenEncoder.EncodeFen(boardCopy).FullFen;

        var result = await grain.PlayMoveAsync(_player.UserId, new MoveKey(move), ApiTestBase.CT);

        result.IsError.Should().BeFalse();

        var legalMoves = _core.GetLegalMoves(_state.CurrentGame.Core);
        MoveSnapshot expectedMoveSnapshot = new(
            Path: MovePath.FromMove(move, GameLogicConstants.BoardWidth),
            Fen: expectedFen,
            NextSideToMove: GameColor.Black,
            San: _sanCalculator.CalculateSan(move, legalMoves.AllMoves),
            TimeLeft: 0
        );
        await _notifierMock
            .Received(1)
            .NotifyPlayerMadeMoveAsync(
                _gameToken,
                ArgEx.FluentAssert<MoveSnapshot>(x =>
                    x.Should().BeEquivalentTo(expectedMoveSnapshot)
                ),
                plyNumber: 1,
                didMoveEndGame: false
            );
    }

    [Fact]
    public async Task PlayMoveAsync_plays_bot_move_when_player_is_white()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain, _whitePlayer);

        Move move = GetLegalMove();
        await grain.PlayMoveAsync(_whitePlayer.UserId, new MoveKey(move), ApiTestBase.CT);

        await AssertBotMoveAsync(_firstBlackBotMove);
    }

    [Fact]
    public async Task PlayMoveAsync_plays_bot_move_when_player_is_black()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain, _blackPlayer);
        await WaitForBotMoveAsync();

        AiEngineMove secondBotMove = new(
            From: new("f1"),
            To: new("f2"),
            Captures: [],
            PromotesTo: null,
            EvalForBot: 1
        );
        _botServiceMock
            .FindBestMoveAsync(
                _state.CurrentGame!.Core.Board,
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(secondBotMove);
        _notifierMock.ClearReceivedCalls();

        Move move = GetLegalMove();
        await grain.PlayMoveAsync(_blackPlayer.UserId, new MoveKey(move), ApiTestBase.CT);

        await AssertBotMoveAsync(secondBotMove);
    }

    [Fact]
    public async Task PlayMoveAsync_ends_game_when_player_ends_game()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain, _whitePlayer);

        AiEngineMove botMove1 = new(
            From: new("g9"),
            To: new("g7"),
            Captures: [],
            PromotesTo: null,
            EvalForBot: 1
        );
        AiEngineMove botMove2 = new(
            From: new("f9"),
            To: new("f6"),
            Captures: [],
            PromotesTo: null,
            EvalForBot: 2
        );
        _botServiceMock
            .FindBestMoveAsync(
                _state.CurrentGame!.Core.Board,
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(botMove1, botMove2);

        Move move1 = new(from: new("f2"), new("f5"), piece: new(PieceType.Queen, GameColor.White));
        Move move2 = new(from: new("e1"), new("j6"), piece: new(PieceType.Queen, GameColor.White));
        Move move3 = new(from: new("j6"), new("f10"), piece: new(PieceType.Queen, GameColor.White));
        await grain.PlayMoveAsync(_whitePlayer.UserId, new(move1), ApiTestBase.CT);
        await AssertBotMoveAsync(botMove1);
        await grain.PlayMoveAsync(_whitePlayer.UserId, new(move2), ApiTestBase.CT);
        await AssertBotMoveAsync(botMove2);
        await grain.PlayMoveAsync(_whitePlayer.UserId, new(move3), ApiTestBase.CT);

        await AssertGameEndedAsync(grain, _gameResultDescriber.KingCaptured(by: GameColor.White));
    }

    [Fact]
    public async Task PlayMoveAsync_ends_game_when_bot_ends_game()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);

        AiEngineMove botMove1 = new(
            From: new("f2"),
            To: new("f5"),
            Captures: [],
            PromotesTo: null,
            EvalForBot: 1
        );
        AiEngineMove botMove2 = new(
            From: new("e1"),
            To: new("j6"),
            Captures: [],
            PromotesTo: null,
            EvalForBot: 2
        );
        AiEngineMove botMove3 = new(
            From: new("j6"),
            To: new("f10"),
            Captures: [new("f10")],
            PromotesTo: null,
            EvalForBot: 3
        );
        _botServiceMock
            .FindBestMoveAsync(
                Arg.Any<IReadOnlyChessBoard>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(botMove1, botMove2, botMove3);
        await StartGameAsync(grain, _blackPlayer, setMove: false);
        await AssertBotMoveAsync(botMove1);

        Move move1 = new(from: new("g9"), new("g7"), piece: new(PieceType.Pawn, GameColor.Black));
        Move move2 = new(from: new("f9"), new("f6"), piece: new(PieceType.Pawn, GameColor.Black));
        await grain.PlayMoveAsync(_blackPlayer.UserId, new(move1), ApiTestBase.CT);
        await AssertBotMoveAsync(botMove2);
        await grain.PlayMoveAsync(_blackPlayer.UserId, new(move2), ApiTestBase.CT);
        await AssertBotMoveAsync(botMove3, didMoveEndGame: true);

        await AssertGameEndedAsync(grain, _gameResultDescriber.KingCaptured(by: GameColor.White));
    }

    [Fact]
    public async Task PlayMoveAsync_returns_error_for_illegal_move()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain);

        var result = await grain.PlayMoveAsync(_player.UserId, new("ILLEGAL"), ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public async Task ResignAsync_ends_game()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain);

        var result = await grain.ResignAsync(_player.UserId, ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        await AssertGameEndedAsync(grain, _gameResultDescriber.Resignation(_player.Color));
    }

    [Fact]
    public async Task ResignAsync_rejects_wrong_user()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain);

        var result = await grain.ResignAsync("wrong user", ApiTestBase.CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task GetStateAsync_returns_the_correct_state()
    {
        var grain = await Silo.CreateGrainAsync<BotGrain>(_gameToken);
        await StartGameAsync(grain, _whitePlayer, botType: BotType.LobotomizedAnarchyBot);

        var result = await grain.GetStateAsync(ApiTestBase.CT);

        result.IsError.Should().BeFalse();
        var legalMoves = _core.GetLegalMoves(_state.CurrentGame!.Core);
        BotGameState expectedState = new(
            WhitePlayer: _whitePlayer,
            BlackPlayer: _lobotomizedAnarchyBot.CreateBotPlayer(GameColor.Black),
            BotColor: GameColor.Black,
            BotType: BotType.LobotomizedAnarchyBot,
            SideToMove: GameColor.White,
            InitialFen: _state.CurrentGame.InitialFen,
            MoveHistory: [],
            LegalMoves: legalMoves.MovePaths,
            ResultData: null
        );
        result.Value.Should().BeEquivalentTo(expectedState);
    }

    private Move GetLegalMove() => _core.GetLegalMoves(_state.CurrentGame!.Core).AllMoves.First();

    private Task StartGameAsync(
        BotGrain grain,
        GamePlayer? player = null,
        BotType botType = BotType.AnarchyBot,
        bool setMove = true
    )
    {
        player ??= _player;
        if (setMove)
        {
            _botServiceMock
                .FindBestMoveAsync(
                    Arg.Any<IReadOnlyChessBoard>(),
                    Arg.Any<int>(),
                    Arg.Any<CancellationToken>()
                )
                .Returns(player.Color is GameColor.White ? _firstBlackBotMove : _firstWhiteBotMove);
        }
        Silo.AddProbe(id => id.ToString() == _gameToken ? grain : Substitute.For<IBotGrain>());
        return grain.StartGameAsync(player ?? _player, botType, ApiTestBase.CT);
    }

    private async Task AssertBotMoveAsync(AiEngineMove expectedMove, bool didMoveEndGame = false)
    {
        await Wait.UntilAsync(
            () =>
                _notifierMock
                    .Received(1)
                    .NotifyBotMadeMoveAsync(
                        _gameToken,
                        Arg.Is<MoveSnapshot>(snapshot =>
                            snapshot.Path.FromIdx == expectedMove.From.AsIdx()
                            && snapshot.Path.ToIdx == expectedMove.To.AsIdx()
                            && snapshot.Path.PromotesTo == expectedMove.PromotesTo
                            && (
                                snapshot.Path.CapturedIdxs ?? Enumerable.Empty<byte>()
                            ).SequenceEqual(
                                (
                                    expectedMove.Captures ?? Enumerable.Empty<AlgebraicPoint>()
                                ).Select(capture => capture.AsIdx())
                            )
                        ),
                        plyNumber: _state.CurrentGame!.MoveHistory.Moves.Count,
                        compressedLegalMoves: Arg.Any<CompressedMoves>(),
                        evalForBot: expectedMove.EvalForBot,
                        didMoveEndGame: didMoveEndGame
                    )
        );

        MoveSnapshot lastMove = _state.CurrentGame!.MoveHistory.Moves[^1];
        lastMove.Path.FromIdx.Should().Be(expectedMove.From.AsIdx());
        lastMove.Path.ToIdx.Should().Be(expectedMove.To.AsIdx());
        lastMove.TimeLeft.Should().Be(0);

        _state.CurrentGame.LastEval.Should().Be(expectedMove.EvalForBot);
    }

    private async Task AssertGameEndedAsync(BotGrain grain, GameEndStatus expectedEndStatus)
    {
        await _notifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                _gameToken,
                result: ArgEx.FluentAssert<GameResultData>(
                    (x) =>
                    {
                        x?.Result.Should().Be(expectedEndStatus.Result);
                        x?.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);
                    }
                )
            );

        var gameStateResult = await grain.GetStateAsync();
        gameStateResult.IsError.Should().BeFalse();
        var gameState = gameStateResult.Value;

        gameState.ResultData.Should().NotBeNull();
        gameState.ResultData.Result.Should().Be(expectedEndStatus.Result);
        gameState.ResultData.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);

        var inDb = await ApiTestBase.DbContext.GameArchives.FirstAsync(ApiTestBase.CT);
        inDb.GameToken.Should().Be(_gameToken);
        inDb.IsBotGame.Should().BeTrue();
    }

    private async Task WaitForBotMoveAsync()
    {
        await Wait.UntilAsync(
            () =>
                _notifierMock
                    .ReceivedWithAnyArgs(1)
                    .NotifyBotMadeMoveAsync(default, default!, default, default, default, default)
        );
        _notifierMock.ClearReceivedCalls();
    }
}
