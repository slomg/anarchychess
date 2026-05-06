using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class QueentumTunnelingRuleTests
{
    private readonly Piece _whiteQueen = PieceFactory.White(PieceType.Queen);
    private readonly Piece _whiteAntiqueen = PieceFactory.White(PieceType.Antiqueen);

    private readonly Piece _blackQueen = PieceFactory.Black(PieceType.Queen);
    private readonly Piece _blackAntiqueen = PieceFactory.Black(PieceType.Antiqueen);

    [Fact]
    public void Evalute_creates_no_moves_if_no_tunnel_piece_found()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e1")] = _whiteQueen,
                [new("i1")] = PieceFactory.White(PieceType.Horsey),
            }
        );
        QueentumTunnelingRule rule = new(tunnelWith: PieceType.Antiqueen);

        List<Move> result = [.. rule.Evaluate(board, new("e1"), _whiteQueen)];

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_creates_no_moves_if_tunnel_piece_is_incorrect_color()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e1")] = _whiteQueen,
                [new("i1")] = _blackAntiqueen,
            }
        );
        QueentumTunnelingRule rule = new(tunnelWith: PieceType.Antiqueen);

        List<Move> result = [.. rule.Evaluate(board, new("e1"), _whiteQueen)];

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_finds_all_tunneling_pieces_and_creates_moves()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e1")] = _whiteQueen,
                [new("i1")] = _whiteAntiqueen,
                [new("f6")] = _whiteAntiqueen,
                [new("b7")] = _whiteAntiqueen,
            }
        );
        QueentumTunnelingRule rule = new(tunnelWith: PieceType.Antiqueen);

        List<Move> result = [.. rule.Evaluate(board, new("e1"), _whiteQueen)];

        result
            .Should()
            .BeEquivalentTo(
                [
                    new Move(
                        from: new("e1"),
                        to: new("i1"),
                        piece: _whiteQueen,
                        sideEffects: [new(From: new("i1"), To: new("e1"), Piece: _whiteAntiqueen)],
                        specialMoveType: SpecialMoveType.QueentumTunnel
                    ),
                    new Move(
                        from: new("e1"),
                        to: new("f6"),
                        piece: _whiteQueen,
                        sideEffects: [new(From: new("f6"), To: new("e1"), Piece: _whiteAntiqueen)],
                        specialMoveType: SpecialMoveType.QueentumTunnel
                    ),
                    new Move(
                        from: new("e1"),
                        to: new("b7"),
                        piece: _whiteQueen,
                        sideEffects: [new(From: new("b7"), To: new("e1"), Piece: _whiteAntiqueen)],
                        specialMoveType: SpecialMoveType.QueentumTunnel
                    ),
                ]
            );
    }

    [Fact]
    public void Evalute_works_with_black_pieces()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("d6")] = _blackQueen,
                [new("g3")] = _blackAntiqueen,
                [new("i1")] = _whiteAntiqueen,
            }
        );
        QueentumTunnelingRule rule = new(tunnelWith: PieceType.Antiqueen);

        List<Move> result = [.. rule.Evaluate(board, new("d6"), _blackQueen)];

        result
            .Should()
            .BeEquivalentTo(
                [
                    new Move(
                        from: new("d6"),
                        to: new("g3"),
                        piece: _blackQueen,
                        sideEffects: [new(From: new("g3"), To: new("d6"), Piece: _blackAntiqueen)],
                        specialMoveType: SpecialMoveType.QueentumTunnel
                    ),
                ]
            );
    }

    [Fact]
    public void Evaluate_ignores_stunned_pieces()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e1")] = _whiteQueen,
                [new("i5")] = _whiteAntiqueen,
                [new("f5")] = _whiteAntiqueen,
            },
            stunnedPieces: new() { [new("i5")] = 3 }
        );

        QueentumTunnelingRule rule = new(tunnelWith: PieceType.Antiqueen);

        List<Move> result = [.. rule.Evaluate(board, new("e1"), _whiteQueen)];

        result
            .Should()
            .BeEquivalentTo(
                [
                    new Move(
                        from: new("e1"),
                        to: new("f5"),
                        piece: _whiteQueen,
                        sideEffects: [new(From: new("f5"), To: new("e1"), Piece: _whiteAntiqueen)],
                        specialMoveType: SpecialMoveType.QueentumTunnel
                    ),
                ]
            );
    }
}
