using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.MovementBehaviours;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class LaBastardaRuleTests
{
    private readonly Piece _whiteKing = PieceFactory.White(PieceType.King);
    private readonly Piece _whiteQueen = PieceFactory.White(PieceType.Queen);

    private readonly Piece _blackKing = PieceFactory.Black(PieceType.King);
    private readonly Piece _blackQueen = PieceFactory.Black(PieceType.Queen);

    private readonly LaBastardaRule _rule = new(
        new CaptureRule(
            new StepBehaviour(new Offset(X: 0, Y: 1)),
            new StepBehaviour(new Offset(X: 0, Y: -1)),
            new StepBehaviour(new Offset(X: 1, Y: 1)),
            new StepBehaviour(new Offset(X: 1, Y: 0)),
            new StepBehaviour(new Offset(X: 1, Y: -1)),
            new StepBehaviour(new Offset(X: -1, Y: 1)),
            new StepBehaviour(new Offset(X: -1, Y: 0)),
            new StepBehaviour(new Offset(X: -1, Y: -1))
        )
    );

    [Fact]
    public void Evaluate_creates_no_pawn_if_no_nearby_queen()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f5")] = _whiteKing,
                [new("d5")] = _blackQueen,
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f5"), _whiteKing)];

        result.Where(m => m.PieceSpawns.Any()).ToList().Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_creates_no_pawn_if_ally_queen()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f5")] = _whiteKing,
                [new("e5")] = _whiteQueen,
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f5"), _whiteKing)];

        result.Where(m => m.PieceSpawns.Any()).ToList().Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_creates_no_pawn_if_stunned_queen()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f5")] = _whiteKing,
                [new("e5")] = _blackQueen,
            },
            stunnedPieces: new() { [new("e5")] = 3 }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f5"), _whiteKing)];

        result.Where(m => m.PieceSpawns.Any()).ToList().Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_creates_pawns_when_opponent_queen_nearby()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f5")] = _whiteKing,
                [new("e5")] = _blackQueen,
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f5"), _whiteKing)];

        result
            .Where(m => m.PieceSpawns.Any())
            .ToList()
            .Should()
            .BeEquivalentTo([
                new Move(
                    from: new("f5"),
                    to: new("g4"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g5"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g6"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
            ]);
    }

    [Fact]
    public void Evaluate_create_pawns_if_multiple_queens()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f5")] = _whiteKing,
                [new("e5")] = _blackQueen,
                [new("g4")] = _blackQueen,
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f5"), _whiteKing)];

        result
            .Where(m => m.PieceSpawns.Any())
            .ToList()
            .Should()
            .BeEquivalentTo([
                new Move(
                    from: new("f5"),
                    to: new("e4"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("e5"),
                    piece: _whiteKing,
                    captures: [new MoveCapture(new("e5"), board)],
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("e6"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("f6"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g6"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g5"),
                    piece: _whiteKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g4"),
                    piece: _whiteKing,
                    captures: [new MoveCapture(new("g4"), board)],
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.Black,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
            ]);
    }

    [Fact]
    public void Evaluate_works_with_black_pieces()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f5")] = _blackKing,
                [new("e5")] = _whiteQueen,
            }
        );

        List<Move> result = [.. _rule.Evaluate(board, new("f5"), _blackKing)];

        result
            .Where(m => m.PieceSpawns.Any())
            .ToList()
            .Should()
            .BeEquivalentTo([
                new Move(
                    from: new("f5"),
                    to: new("g4"),
                    piece: _blackKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.White,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g5"),
                    piece: _blackKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.White,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
                new Move(
                    from: new("f5"),
                    to: new("g6"),
                    piece: _blackKing,
                    pieceSpawns:
                    [
                        new PieceSpawn(
                            Type: PieceType.UnderagePawn,
                            Color: GameColor.White,
                            Position: new("f5")
                        ),
                    ],
                    specialMoveType: SpecialMoveType.LaBastarda
                ),
            ]);
    }
}
