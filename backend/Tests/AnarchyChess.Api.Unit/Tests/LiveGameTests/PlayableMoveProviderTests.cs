using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.EngineShared.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class PlayableMoveProviderTests
{
    private readonly ILegalMoveCalculator _legalMoveCalculatorMock =
        Substitute.For<ILegalMoveCalculator>();
    private readonly ChessBoard _board = new();

    private readonly PlayableMoveProvider _playableMoveProvider;

    public PlayableMoveProviderTests()
    {
        _playableMoveProvider = new(_legalMoveCalculatorMock);
        _board.PlacePiece(new("a1"), PieceFactory.White(PieceType.King));
        _board.PlacePiece(new("a8"), PieceFactory.Black(PieceType.King));
    }

    [Fact]
    public void CalculateAllPlayableMoves_returns_all_moves()
    {
        var move1 = new MoveFaker().Generate();
        var move2 = new MoveFaker().Generate();
        Move[] allMoves = [move1, move2];

        _legalMoveCalculatorMock.CalculateAllLegalMoves(_board).Returns(allMoves);

        var result = _playableMoveProvider.CalculateAllPlayableMoves(_board);

        var moveMap = allMoves.ToDictionary(x => new MoveKey(x));
        var movePaths = allMoves
            .Select(m => MovePath.FromMove(m, _board.Width, new MoveKey(m)))
            .ToList();

        var expected = new LegalMoveSet(MoveMap: moveMap, MovePaths: movePaths);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    public void CalculateAllPlayableMoves_returns_empty_when_one_king_is_missing(
        GameColor sideMissing
    )
    {
        ChessBoard board = new();
        board.PlacePiece(new("a1"), PieceFactory.White(PieceType.Antiqueen));
        board.PlacePiece(new("a2"), PieceFactory.Black(PieceType.Rook));
        board.PlacePiece(new("a2"), new Piece(PieceType.King, Color: sideMissing.Invert()));

        var result = _playableMoveProvider.CalculateAllPlayableMoves(board);

        result.Should().BeEquivalentTo(new LegalMoveSet());
        _legalMoveCalculatorMock.DidNotReceiveWithAnyArgs().CalculateAllLegalMoves(default!);
    }

    [Fact]
    public void CalculateAllPlayableMoves_returns_only_max_priority_moves()
    {
        var move1 = new MoveFaker()
            .RuleFor(x => x.ForcedPriority, ForcedMovePriority.None)
            .Generate();
        var move2 = new MoveFaker()
            .RuleFor(x => x.ForcedPriority, ForcedMovePriority.UnderagePawn)
            .Generate();
        var move3 = new MoveFaker()
            .RuleFor(x => x.ForcedPriority, ForcedMovePriority.EnPassant)
            .Generate();
        MoveKey expectedMoveKey = new(move3);

        _legalMoveCalculatorMock.CalculateAllLegalMoves(_board).Returns([move1, move2, move3]);

        var result = _playableMoveProvider.CalculateAllPlayableMoves(_board);

        LegalMoveSet expected = new(
            MoveMap: new Dictionary<MoveKey, Move> { [new MoveKey(move3)] = move3 },
            MovePaths: [MovePath.FromMove(move3, _board.Width, expectedMoveKey)]
        );

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetPieceMoveByKey_returns_null_when_one_king_is_missing()
    {
        ChessBoard board = new();
        board.PlacePiece(new("a2"), PieceFactory.White(PieceType.King));
        MoveKey moveKey = "move key";

        var result = _playableMoveProvider.GetPieceMoveByKey(board, new("a1"), moveKey);

        result.Should().BeNull();
        _legalMoveCalculatorMock
            .DidNotReceiveWithAnyArgs()
            .CalculateLegalMovesForPiece(default!, default!);
    }

    [Fact]
    public void GetPieceMoveByKey_returns_correct_move_when_move_exists_for_piece()
    {
        var move1 = new MoveFaker().RuleFor(x => x.From, new AlgebraicPoint("a1")).Generate();
        var move2 = new MoveFaker().RuleFor(x => x.From, new AlgebraicPoint("a2")).Generate();
        var move3 = new MoveFaker().RuleFor(x => x.From, new AlgebraicPoint("a3")).Generate();

        var position = move2.From;
        MoveKey moveKey = new(move2);
        _legalMoveCalculatorMock
            .CalculateLegalMovesForPiece(_board, position)
            .Returns([move1, move2, move3]);

        var result = _playableMoveProvider.GetPieceMoveByKey(_board, position, moveKey);

        result.Should().Be(move2);
        _legalMoveCalculatorMock.DidNotReceiveWithAnyArgs().CalculateForeverRules(default!);
    }

    [Fact]
    public void GetPieceMoveByKey_returns_forever_rule_move_when_not_found_in_piece_moves()
    {
        var piecePosition = new AlgebraicPoint("a1");

        var pieceMove = new MoveFaker().RuleFor(x => x.From, piecePosition).Generate();

        var foreverRuleMove = new MoveFaker().Generate();
        MoveKey foreverRuleKey = new(foreverRuleMove);

        _legalMoveCalculatorMock
            .CalculateLegalMovesForPiece(_board, piecePosition)
            .Returns([pieceMove]);

        _legalMoveCalculatorMock.CalculateForeverRules(_board).Returns([foreverRuleMove]);

        var result = _playableMoveProvider.GetPieceMoveByKey(_board, piecePosition, foreverRuleKey);

        result.Should().Be(foreverRuleMove);
    }

    [Fact]
    public void GetPieceMoveByKey_returns_null_when_move_is_not_found()
    {
        AlgebraicPoint position = new("a1");
        _legalMoveCalculatorMock
            .CalculateLegalMovesForPiece(_board, position)
            .Returns(new MoveFaker().Generate(3));
        _legalMoveCalculatorMock.CalculateForeverRules(_board).Returns(new MoveFaker().Generate(3));

        var result = _playableMoveProvider.GetPieceMoveByKey(_board, position, "bad move key");

        result.Should().BeNull();
    }
}
