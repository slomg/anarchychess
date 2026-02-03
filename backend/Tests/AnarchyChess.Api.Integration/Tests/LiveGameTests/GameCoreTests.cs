using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.TestData;
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
    public void StartGame_sets_initial_state_correctly()
    {
        GameCoreState state = new();

        var initialFen = _gameCore.StartGame(state);

        initialFen.FullFen.Should().Be(GameTestData.InitialFen);
        state.AutoDrawState.FenOccurrences.Should().ContainSingle();
        state.AutoDrawState.FenOccurrences[initialFen.FullFen].Should().Be(1);
        state.LegalMoves.Should().NotBeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_moves_the_piece_and_updates_legal_moves()
    {
        var state = StartGame();
        var moveKey = new MoveKey(new("e2"), new("e4"));
        var result = _gameCore.MakeMove(moveKey, state);

        result.IsError.Should().BeFalse();
        _gameCore.SideToMove(state).Should().Be(GameColor.Black);

        var legalMoves = _gameCore.GetLegalMoves(state);
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
    public void MakeMove_detects_draw()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("c1"), PieceFactory.Black(PieceType.King));
        var state = StartGame(new() { Board = board });

        List<MoveKey> repetitionMoves =
        [
            new MoveKey(new("a1"), new("a2")),
            new MoveKey(new("c1"), new("c2")),
            new MoveKey(new("a2"), new("a1")),
            new MoveKey(new("c2"), new("c1")),
        ];

        var nonDrawResult = MakeMoves(state, repetitionMoves);
        nonDrawResult.EndStatus.Should().BeNull();

        // now it should be a draw, the initial position happened 3 times
        var drawResult = MakeMoves(state, repetitionMoves);
        drawResult.EndStatus.Should().Be(_resultDescriber.ThreeFold());
        drawResult.San.Should().Be("Kc1½");
        var legalMoves = _gameCore.GetLegalMoves(state);
        legalMoves.Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_detects_king_capture()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Queen));
        board.PlacePiece(new("a2"), PieceFactory.Black(PieceType.King));
        board.PlacePiece(new("a3"), PieceFactory.Black(PieceType.Rook));
        board.PlacePiece(new("a4"), PieceFactory.White(PieceType.King));
        var state = StartGame(new() { Board = board });

        var result = MakeMoves(state, new MoveKey(new("a1"), new("a2")));

        result.EndStatus.Should().Be(_resultDescriber.KingCaptured(by: GameColor.White));
        result.San.Should().Be("Qxa2#");
        var legalMoves = _gameCore.GetLegalMoves(state);
        legalMoves.Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_detects_self_capture()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.Horsey));
        board.PlacePiece(new("a3"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("c1"), PieceFactory.Black(PieceType.King));
        var state = StartGame(new() { Board = board });

        // white knooklear fusion explosion captures the white king
        var result = MakeMoves(
            state,
            new MoveKey(new("a3"), new("a2"), promotesTo: PieceType.Knook)
        );

        result.EndStatus.Should().Be(_resultDescriber.KingSelfCapture(by: GameColor.White));
        result.San.Should().Be("Rxa2xa1=N#");
        var legalMoves = _gameCore.GetLegalMoves(state);
        legalMoves.Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_detects_mutual_king_capture()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("b1"), PieceFactory.White(PieceType.Horsey));
        board.PlacePiece(new("b2"), PieceFactory.White(PieceType.Rook));
        board.PlacePiece(new("c1"), PieceFactory.Black(PieceType.King));
        var state = StartGame(new() { Board = board });

        var result = MakeMoves(
            state,
            new MoveKey(new("b2"), new("b1"), promotesTo: PieceType.Knook)
        );

        result.EndStatus.Should().Be(_resultDescriber.MutualKingCapture());
        result.San.Should().Be("Rxb1xc1xa1=N½");
        var legalMoves = _gameCore.GetLegalMoves(state);
        legalMoves.Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_returns_error_for_illegal_move()
    {
        var state = StartGame();
        var result = _gameCore.MakeMove(new MoveKey(new("a1"), new("a9")), state);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public void MakeMove_returns_normal_san_for_non_king_capture()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        board.PlacePiece(new("c1"), PieceFactory.Black(PieceType.King));
        board.PlacePiece(new("d1"), PieceFactory.White(PieceType.Rook));
        var state = StartGame(new() { Board = board });

        var result = MakeMoves(state, new MoveKey(new("d1"), new("d4")));

        result.EndStatus.Should().BeNull();
        result.San.Should().Be("Rd4");
        var legalMoves = _gameCore.GetLegalMoves(state);
        legalMoves.Should().NotBeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void GetReadOnlyBoard_returns_the_chessboard()
    {
        var state = StartGame();

        var result = _gameCore.GetReadOnlyBoard(state);

        result.Should().Be(state.Board);
    }

    [Fact]
    public void RemovePieces_removes_specified_pieces_from_board()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White());
        board.PlacePiece(new("a2"), PieceFactory.Black());
        board.PlacePiece(new("a3"), PieceFactory.White());
        board.PlacePiece(new("a4"), PieceFactory.Black());
        var state = StartGame(new() { Board = board });
        AlgebraicPoint toRemove = new("a1");

        ChessBoard expectedBoard = new(board);
        expectedBoard.RemovePiece(new("a1"));

        var newLegalMoves = new LegalMoveSet(
            MoveMap: new Dictionary<MoveKey, Move>(),
            MovePaths: new MovePathFaker().Generate(3)
        );
        _gameCore.RemovePiece(toRemove, newLegalMoves, state);

        state.Board.Should().BeEquivalentTo(expectedBoard);
        state.LegalMoves.Should().BeEquivalentTo(newLegalMoves);
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
