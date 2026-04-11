using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class ThrowingRuleTests
{
    private readonly ThrowingRule _rule = new();

    private readonly Piece _whitePawn = PieceFactory.White(PieceType.Pawn);
    private readonly Piece _blackPawn = PieceFactory.Black(PieceType.Pawn);

    private static List<Move> CreateExpectedMoves(
        Piece piece,
        AlgebraicPoint origin,
        ChessBoard board,
        IEnumerable<AlgebraicPoint> expectedDests,
        IEnumerable<AlgebraicPoint> triggerSquares
    ) =>
        [
            .. expectedDests.Select(dest =>
            {
                List<MoveStun> stuns =
                [
                    new MoveStun(Position: origin, Piece: piece, StunForTurns: 2),
                ];
                List<MoveCapture> captures = [];
                if (board.TryGetPieceAt(dest, out var hitPiece))
                {
                    stuns.Add(new MoveStun(Position: dest, Piece: hitPiece, StunForTurns: 4));
                    captures.Add(new MoveCapture(piece, origin));
                }

                return new Move(
                    from: origin,
                    to: dest,
                    piece,
                    specialMoveType: SpecialMoveType.Throw,
                    stuns: stuns,
                    captures: captures,
                    triggerSquares: triggerSquares
                );
            }),
        ];

    [Fact]
    public void Evaluate_creates_no_moves_if_there_is_no_piece_behind()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>() { [new("f4")] = _whitePawn }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_creates_no_move_if_there_is_an_enemy_piece_behind()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("e3")] = PieceFactory.Black(PieceType.Rook),
                [new("f3")] = PieceFactory.Black(PieceType.Horsey),
                [new("g3")] = PieceFactory.Black(PieceType.Bishop),
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.SterilePawn)]
    [InlineData(PieceType.UnderagePawn)]
    public void Evaluate_creates_no_move_if_the_piece_behind_is_too_weak(PieceType piece)
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("e3")] = PieceFactory.White(piece),
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_creates_forward_throw_moves()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("f3")] = PieceFactory.White(PieceType.Horsey),
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        List<AlgebraicPoint> expectedDests =
        [
            new("e5"),
            new("f5"),
            new("g5"),
            new("e6"),
            new("f6"),
            new("g6"),
            new("e7"),
            new("f7"),
            new("g7"),
            new("e8"),
            new("f8"),
            new("g8"),
            new("e9"),
            new("f9"),
            new("g9"),
        ];
        result
            .Should()
            .BeEquivalentTo(
                CreateExpectedMoves(
                    _whitePawn,
                    origin: new("f4"),
                    board,
                    expectedDests: expectedDests,
                    triggerSquares: [new("f3")]
                )
            );
    }

    [Fact]
    public void Evaluate_creates_left_throw_moves()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("g3")] = PieceFactory.White(PieceType.Bishop),
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        List<AlgebraicPoint> expectedDests =
        [
            new("e4"),
            new("e5"),
            new("e6"),
            new("d5"),
            new("d6"),
            new("d7"),
            new("c6"),
            new("c7"),
            new("c8"),
            new("b7"),
            new("b8"),
            new("b9"),
            new("a8"),
            new("a9"),
        ];
        result
            .Should()
            .BeEquivalentTo(
                CreateExpectedMoves(
                    _whitePawn,
                    origin: new("f4"),
                    board,
                    expectedDests: expectedDests,
                    triggerSquares: [new("g3")]
                )
            );
    }

    [Fact]
    public void Evaluate_creates_right_throw_moves()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("e3")] = PieceFactory.White(PieceType.Rook),
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        List<AlgebraicPoint> expectedDests =
        [
            new("g4"),
            new("g5"),
            new("g6"),
            new("h5"),
            new("h6"),
            new("h7"),
            new("i6"),
            new("i7"),
            new("i8"),
            new("j7"),
            new("j8"),
            new("j9"),
        ];
        result
            .Should()
            .BeEquivalentTo(
                CreateExpectedMoves(
                    _whitePawn,
                    origin: new("f4"),
                    board,
                    expectedDests: expectedDests,
                    triggerSquares: [new("e3")]
                )
            );
    }

    [Fact]
    public void Evaluate_creates_forward_black_throw_moves()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f6")] = _blackPawn,
                [new("f7")] = PieceFactory.Black(PieceType.Rook),
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f6"), _blackPawn)];

        List<AlgebraicPoint> expectedDests =
        [
            new("e5"),
            new("f5"),
            new("g5"),
            new("e4"),
            new("f4"),
            new("g4"),
            new("e3"),
            new("f3"),
            new("g3"),
            new("e2"),
            new("f2"),
            new("g2"),
        ];
        result
            .Should()
            .BeEquivalentTo(
                CreateExpectedMoves(
                    _blackPawn,
                    origin: new("f6"),
                    board,
                    expectedDests: expectedDests,
                    triggerSquares: [new("f7")]
                )
            );
    }

    [Fact]
    public void Evaluate_stuns_enemy_pieces()
    {
        Piece stunnedPiece = PieceFactory.Black();
        Piece friendlyPiece = PieceFactory.White();

        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("f3")] = PieceFactory.White(PieceType.Rook),

                [new("e6")] = stunnedPiece,
                [new("g8")] = friendlyPiece,
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        List<AlgebraicPoint> expectedDests =
        [
            new("e5"),
            new("f5"),
            new("g5"),
            new("e6"),
            new("f6"),
            new("g6"),
            new("e7"),
            new("f7"),
            new("g7"),
            new("e8"),
            new("f8"),
            new("e9"),
            new("f9"),
            new("g9"),
        ];
        result
            .Should()
            .BeEquivalentTo(
                CreateExpectedMoves(
                    _whitePawn,
                    origin: new("f4"),
                    board,
                    expectedDests: expectedDests,
                    triggerSquares: [new("f3")]
                )
            );
    }

    [Fact]
    public void Evaluate_creates_no_moves_if_thrower_is_stunned()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f4")] = _whitePawn,
                [new("f3")] = PieceFactory.White(PieceType.Rook),
            },
            stunnedPieces: new Dictionary<AlgebraicPoint, int> { [new("f3")] = 1 }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f4"), _whitePawn)];

        result.Should().BeEmpty();
    }
}
