using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class BouncingRuleTests
{
    private readonly IPieceMovementRule _subRuleMock = Substitute.For<IPieceMovementRule>();
    private readonly Piece _piece = PieceFactory.White();

    [Fact]
    public void Evaluate_returns_no_moves_when_subrule_returns_no_moves()
    {
        ChessBoard board = new();
        AlgebraicPoint origin = new("e5");
        board.PlacePiece(origin, _piece);

        _subRuleMock.Evaluate(board, origin, _piece).Returns([]);

        BouncingRule rule = new(
            initialOffset: new Offset(1, 0),
            ruleCreator: (_, _) => _subRuleMock,
            stopBouncingPredicate: (_, _) => false
        );
        var moves = rule.Evaluate(board, origin, _piece).ToList();

        moves.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_stops_once_we_revisit_a_position()
    {
        ChessBoard board = new();
        AlgebraicPoint origin = new("c5");
        board.PlacePiece(origin, _piece);

        List<Move> firstBounce =
        [
            new Move(origin, new AlgebraicPoint("b5"), _piece),
            new Move(origin, new AlgebraicPoint("a5"), _piece),
        ];
        var lastFirstBounce = firstBounce.Last();
        _subRuleMock.Evaluate(board, origin, _piece).Returns(firstBounce);
        _subRuleMock
            .Evaluate(board, lastFirstBounce.To, _piece)
            .Returns(
                [
                    new Move(lastFirstBounce.To, new AlgebraicPoint("b5"), _piece),
                    new Move(lastFirstBounce.To, new AlgebraicPoint("c5"), _piece),
                    new Move(lastFirstBounce.To, new AlgebraicPoint("d5"), _piece),
                ]
            );

        BouncingRule rule = new(
            initialOffset: new Offset(-1, 0),
            ruleCreator: (_, _) => _subRuleMock,
            stopBouncingPredicate: (_, _) => false
        );
        var moves = rule.Evaluate(board, origin, _piece).ToList();

        moves.Should().BeEquivalentTo(firstBounce);
    }

    [Fact]
    public void Evaluate_stops_when_stop_predicate_is_met()
    {
        ChessBoard board = new();
        AlgebraicPoint origin = new("c5");
        board.PlacePiece(origin, _piece);

        List<Move> firstBounce =
        [
            new Move(origin, new AlgebraicPoint("b5"), _piece),
            new Move(origin, new AlgebraicPoint("a5"), _piece),
        ];
        var lastFirstBounce = firstBounce.Last();
        List<Move> secondBounce =
        [
            new Move(lastFirstBounce.To, new AlgebraicPoint("b6"), _piece),
            new Move(lastFirstBounce.To, new AlgebraicPoint("a6"), _piece),
        ];
        var lastSecondBounce = secondBounce.Last();

        _subRuleMock.Evaluate(board, origin, _piece).Returns(firstBounce);
        _subRuleMock.Evaluate(board, lastFirstBounce.To, _piece).Returns(secondBounce);
        _subRuleMock
            .Evaluate(board, secondBounce.Last().To, _piece)
            .Returns(
                [
                    new Move(lastSecondBounce.To, new AlgebraicPoint("b7"), _piece),
                    new Move(lastSecondBounce.To, new AlgebraicPoint("a7"), _piece),
                ]
            );

        BouncingRule rule = new(
            initialOffset: new Offset(-1, 0),
            ruleCreator: (_, _) => _subRuleMock,
            stopBouncingPredicate: (_, move) =>
                move.To == lastSecondBounce.To && move.From == origin
        );
        var moves = rule.Evaluate(board, origin, _piece).ToList();

        moves
            .Should()
            .BeEquivalentTo(
                [
                    .. firstBounce,
                    .. secondBounce.Select(move =>
                        move with
                        {
                            From = origin,
                            IntermediateSquares =
                            [
                                new IntermediateSquare(
                                    Position: lastFirstBounce.To,
                                    IsCapture: false
                                ),
                            ],
                        }
                    ),
                ]
            );
    }

    [Fact]
    public void Evaluate_bounces_off_walls()
    {
        ChessBoard board = new(width: 5, height: 5);
        AlgebraicPoint origin = new("b2");
        board.PlacePiece(origin, _piece);

        List<Move> firstBounce = [new Move(origin, new AlgebraicPoint("a3"), _piece)];
        var lastFirstBounce = firstBounce.Last();
        List<Move> secondBounce =
        [
            new Move(lastFirstBounce.To, new AlgebraicPoint("b4"), _piece),
            new Move(lastFirstBounce.To, new AlgebraicPoint("c5"), _piece),
        ];
        var lastSecondBounce = secondBounce.Last();
        List<Move> thirdBounce =
        [
            new Move(lastSecondBounce.To, new AlgebraicPoint("d4"), _piece),
            new Move(lastSecondBounce.To, new AlgebraicPoint("e3"), _piece),
        ];
        var lastThirdBounce = thirdBounce.Last();
        List<Move> forthBounce =
        [
            new Move(lastThirdBounce.To, new AlgebraicPoint("d2"), _piece),
            new Move(lastThirdBounce.To, new AlgebraicPoint("c1"), _piece),
        ];
        var lastForthBounce = forthBounce.Last();
        List<Move> firthBounce =
        [
            new Move(lastForthBounce.To, new AlgebraicPoint("b2"), _piece),
            new Move(lastForthBounce.To, new AlgebraicPoint("a3"), _piece),
        ];

        _subRuleMock.Evaluate(board, origin, _piece).Returns(firstBounce);
        _subRuleMock.Evaluate(board, lastFirstBounce.To, _piece).Returns(secondBounce);
        _subRuleMock.Evaluate(board, lastSecondBounce.To, _piece).Returns(thirdBounce);
        _subRuleMock.Evaluate(board, lastThirdBounce.To, _piece).Returns(forthBounce);
        _subRuleMock.Evaluate(board, lastForthBounce.To, _piece).Returns(firthBounce);

        List<Offset> receivedOffsets = [];
        BouncingRule rule = new(
            initialOffset: new Offset(-1, 1),
            ruleCreator: (_, offset) =>
            {
                receivedOffsets.Add(offset);
                return _subRuleMock;
            },
            stopBouncingPredicate: (_, _) => false
        );
        var moves = rule.Evaluate(board, origin, _piece).ToList();

        moves
            .Should()
            .BeEquivalentTo(
                [
                    .. firstBounce,
                    .. secondBounce.Select(move =>
                        move with
                        {
                            From = origin,
                            IntermediateSquares =
                            [
                                new IntermediateSquare(
                                    Position: lastFirstBounce.To,
                                    IsCapture: false
                                ),
                            ],
                        }
                    ),
                    .. thirdBounce.Select(move =>
                        move with
                        {
                            From = origin,
                            IntermediateSquares =
                            [
                                new IntermediateSquare(
                                    Position: lastFirstBounce.To,
                                    IsCapture: false
                                ),
                                new IntermediateSquare(
                                    Position: lastSecondBounce.To,
                                    IsCapture: false
                                ),
                            ],
                        }
                    ),
                    .. forthBounce.Select(move =>
                        move with
                        {
                            From = origin,
                            IntermediateSquares =
                            [
                                new IntermediateSquare(
                                    Position: lastFirstBounce.To,
                                    IsCapture: false
                                ),
                                new IntermediateSquare(
                                    Position: lastSecondBounce.To,
                                    IsCapture: false
                                ),
                                new IntermediateSquare(
                                    Position: lastThirdBounce.To,
                                    IsCapture: false
                                ),
                            ],
                        }
                    ),
                ]
            );
        receivedOffsets
            .Should()
            .BeEquivalentTo(
                [
                    new Offset(-1, 1),
                    new Offset(1, 1),
                    new Offset(1, -1),
                    new Offset(-1, -1),
                    new Offset(-1, 1),
                ]
            );
    }

    [Fact]
    public void Evaluate_marks_intermediate_as_capture_when_needed()
    {
        ChessBoard board = new();
        AlgebraicPoint origin = new("b5");
        board.PlacePiece(origin, _piece);

        var enemyPiece = PieceFactory.Black();
        AlgebraicPoint enemyPosition = new("a5");
        board.PlacePiece(enemyPosition, enemyPiece);
        MoveCapture capture = new(enemyPiece, enemyPosition);

        List<Move> firstBounce = [new Move(origin, enemyPosition, _piece, captures: [capture])];
        var lastFirstBounce = firstBounce.Last();
        List<Move> secondBounce =
        [
            new Move(lastFirstBounce.To, new AlgebraicPoint("b6"), _piece),
            new Move(lastFirstBounce.To, new AlgebraicPoint("a6"), _piece),
        ];
        var lastSecondBounce = secondBounce.Last();
        List<Move> thirdBounce = [new Move(lastSecondBounce.To, new AlgebraicPoint("b7"), _piece)];

        _subRuleMock.Evaluate(board, origin, _piece).Returns(firstBounce);
        _subRuleMock.Evaluate(board, lastFirstBounce.To, _piece).Returns(secondBounce);
        _subRuleMock.Evaluate(board, lastSecondBounce.To, _piece).Returns(thirdBounce);

        BouncingRule rule = new(
            initialOffset: new Offset(-1, 0),
            ruleCreator: (_, _) => _subRuleMock,
            stopBouncingPredicate: (_, _) => false
        );
        var moves = rule.Evaluate(board, origin, _piece).ToList();

        moves
            .Should()
            .BeEquivalentTo(
                [
                    .. firstBounce,
                    .. secondBounce.Select(move =>
                        move with
                        {
                            From = origin,
                            Captures = [capture],
                            IntermediateSquares =
                            [
                                new IntermediateSquare(
                                    Position: lastFirstBounce.To,
                                    IsCapture: true
                                ),
                            ],
                        }
                    ),
                    .. thirdBounce.Select(move =>
                        move with
                        {
                            From = origin,
                            Captures = [capture],
                            IntermediateSquares =
                            [
                                new IntermediateSquare(
                                    Position: lastFirstBounce.To,
                                    IsCapture: true
                                ),
                                new IntermediateSquare(
                                    Position: lastSecondBounce.To,
                                    IsCapture: false
                                ),
                            ],
                        }
                    ),
                ]
            );
    }
}
