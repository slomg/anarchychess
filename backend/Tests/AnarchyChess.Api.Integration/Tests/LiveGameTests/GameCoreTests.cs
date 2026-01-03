using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class GameCoreTests : BaseIntegrationTest
{
    private readonly IGameCore _gameCore;
    private readonly GameResultDescriber _resultDescriber = new();

    public GameCoreTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameCore = Scope.ServiceProvider.GetRequiredService<IGameCore>();
    }

    [Fact]
    public void MakeMove_moves_the_piece_and_updates_legal_moves()
    {
        var state = StartGame();
        var moveKey = new MoveKey(new("e2"), new("e4"));
        var result = _gameCore.MakeMove(moveKey, state);

        result.IsError.Should().BeFalse();
        _gameCore.SideToMove(state).Should().Be(GameColor.Black);

        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.Black, state);
        legalMoves.MoveMap.Should().NotBeEmpty();
    }

    [Fact]
    public void MakeMove_allows_multiple_valid_moves_in_sequence()
    {
        var state = StartGame();
        MakeMoves(state, new MoveKey(new("e2"), new("e4")), new MoveKey(new("e9"), new("e7")));

        _gameCore.SideToMove(state).Should().Be(GameColor.White);
    }

    [Fact]
    public void MakeMove_detects_draw_if_occurs()
    {
        var state = StartGame();
        List<MoveKey> repetitionMoves =
        [
            new MoveKey(new("b1"), new("c3")),
            new MoveKey(new("b10"), new("c8")),
            new MoveKey(new("c3"), new("b1")),
            new MoveKey(new("c8"), new("b10")),
        ];

        for (int i = 0; i < 3; i++)
        {
            MakeMoves(state, repetitionMoves);
        }

        var result = MakeMoves(state, repetitionMoves);
        result.EndStatus.Should().Be(_resultDescriber.ThreeFold());
    }

    [Fact]
    public void MakeMove_detects_forced_moves()
    {
        var state = StartGame();
        MakeMoves(
            state,
            new MoveKey(new("f2"), new("f5")),
            new MoveKey(new("f9"), new("f6")),
            new MoveKey(new("g1"), new("c5")),
            new MoveKey(new("a9"), new("a8"))
        );

        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.White, state);
        legalMoves.HasForcedMoves.Should().BeTrue();
        legalMoves.MovePaths.Should().ContainSingle();
        legalMoves.MoveMap.Should().ContainSingle();
    }

    [Fact]
    public void MakeMove_detects_king_capture()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("a2"), PieceFactory.Black(PieceType.King));
        board.PlacePiece(new("a3"), PieceFactory.Black(PieceType.Rook));
        board.PlacePiece(new("a4"), PieceFactory.White(PieceType.King));
        GameCoreState state = new() { Board = board };
        StartGame(state);

        var result = MakeMoves(state, new MoveKey(new("a1"), new("a2")));

        result.EndStatus.Should().Be(_resultDescriber.KingCaptured(GameColor.White));
        result.San.Should().Be("Qxa2#");
        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.Black, state);
        legalMoves.Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    public void StartGame_sets_empty_legal_moves_if_one_side_has_no_king(GameColor sideWithoutKing)
    {
        var sideWithKing = sideWithoutKing.Invert();
        ChessBoard board = new();
        board.PlacePiece(new("a1"), new Piece(PieceType.King, sideWithKing));
        board.PlacePiece(new("a2"), new Piece(PieceType.Rook, sideWithKing));
        board.PlacePiece(new("a3"), new Piece(PieceType.Rook, sideWithoutKing));
        GameCoreState state = new() { Board = board };

        StartGame(state);

        state.LegalMoves.Should().BeEquivalentTo(new LegalMoveSet());
    }

    private MoveResult MakeMoves(GameCoreState state, params IEnumerable<MoveKey> moves)
    {
        MoveResult lastResult = default;
        foreach (var move in moves)
        {
            var result = _gameCore.MakeMove(move, state);
            result.IsError.Should().BeFalse();
            lastResult = result.Value;
        }

        return lastResult;
    }

    private GameCoreState StartGame(GameCoreState? state = null)
    {
        state ??= new();
        _gameCore.StartGame(state);
        return state;
    }
}
