using System.Text;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests.SanNotation;

public class QueentumTunnelNotatorTests
{
    private readonly QueentumTunnelNotator _notator = new(new PieceLetterMap());

    private readonly Piece _queen = PieceFactory.White(PieceType.Queen);
    private readonly Piece _antiqueen = PieceFactory.White(PieceType.Antiqueen);

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.QueentumTunnel);

    [Fact]
    public void Notate_uses_queentum_tunnel_notation()
    {
        Move move = new(
            new("d3"),
            new("h7"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new MoveSideEffect(From: new("d3"), To: new("h7"), Piece: _queen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("QψA");
    }

    [Fact]
    public void Notate_disambiguates_queen_by_rank()
    {
        Move move = new(
            new("b1"),
            new("e5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b1"), _antiqueen)]
        );
        Move ambiguousMove = new(
            new("j1"),
            new("e5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("j1"), _antiqueen)]
        );
        Move antiqueenMove1 = new(
            new("e5"),
            new("b1"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b1"), To: new("e5"), _queen)]
        );
        Move antiqueenMove2 = new(
            new("e5"),
            new("j1"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("j1"), To: new("e5"), _queen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move, ambiguousMove, antiqueenMove1, antiqueenMove2], sb);

        sb.ToString().Should().Be("QbψA");
    }

    [Fact]
    public void Notate_disambiguates_queen_by_file()
    {
        Move move = new(
            new("b1"),
            new("e5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b1"), _antiqueen)]
        );
        Move ambiguousMove = new(
            new("b5"),
            new("e5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b5"), _antiqueen)]
        );
        Move antiqueenMove1 = new(
            new("e5"),
            new("b1"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b1"), To: new("e5"), _queen)]
        );
        Move antiqueenMove2 = new(
            new("e5"),
            new("b5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b5"), To: new("e5"), _queen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move, ambiguousMove, antiqueenMove1, antiqueenMove2], sb);

        sb.ToString().Should().Be("Q1ψA");
    }

    [Fact]
    public void Notate_disambiguates_queen_by_rank_if_ambiguous_but_not_geometry_ambiguous()
    {
        Move move = new(
            new("b1"),
            new("e5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b1"), _antiqueen)]
        );
        Move ambiguousMove = new(
            new("j6"),
            new("e5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("j6"), _antiqueen)]
        );
        Move antiqueenMove1 = new(
            new("e5"),
            new("b1"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b1"), To: new("e5"), _queen)]
        );
        Move antiqueenMove2 = new(
            new("e5"),
            new("j6"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("j6"), To: new("e5"), _queen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move, ambiguousMove, antiqueenMove1, antiqueenMove2], sb);

        sb.ToString().Should().Be("QbψA");
    }

    [Fact]
    public void Notate_disambiguates_antiqueen_by_rank()
    {
        Move move = new(
            new("b1"),
            new("e5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b1"), _queen)]
        );
        Move ambiguousMove = new(
            new("j1"),
            new("e5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("j1"), _queen)]
        );
        Move antiqueenMove1 = new(
            new("e5"),
            new("b1"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b1"), To: new("e5"), _antiqueen)]
        );
        Move antiqueenMove2 = new(
            new("e5"),
            new("j1"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("j1"), To: new("e5"), _antiqueen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move, ambiguousMove], sb);

        sb.ToString().Should().Be("QψAb");
    }

    [Fact]
    public void Notate_disambiguates_antiqueen_by_file()
    {
        Move move = new(
            new("b1"),
            new("e5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b1"), _queen)]
        );
        Move ambiguousMove = new(
            new("b5"),
            new("e5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b5"), _queen)]
        );
        Move antiqueenMove1 = new(
            new("e5"),
            new("b1"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b1"), To: new("e5"), _antiqueen)]
        );
        Move antiqueenMove2 = new(
            new("e5"),
            new("b5"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b5"), To: new("e5"), _antiqueen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move, ambiguousMove, antiqueenMove1, antiqueenMove2], sb);

        sb.ToString().Should().Be("QψA1");
    }

    [Fact]
    public void Notate_disambiguates_antiqueen_by_rank_if_ambiguous_but_not_geometry_ambiguous()
    {
        Move move = new(
            new("b1"),
            new("e5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("b1"), _queen)]
        );
        Move ambiguousMove = new(
            new("j6"),
            new("e5"),
            _antiqueen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("e5"), To: new("j6"), _queen)]
        );
        Move antiqueenMove1 = new(
            new("e5"),
            new("b1"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("b1"), To: new("e5"), _antiqueen)]
        );
        Move antiqueenMove2 = new(
            new("e5"),
            new("j6"),
            _queen,
            specialMoveType: SpecialMoveType.QueentumTunnel,
            sideEffects: [new(From: new("j6"), To: new("e5"), _antiqueen)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move, ambiguousMove, antiqueenMove1, antiqueenMove2], sb);

        sb.ToString().Should().Be("QψAb");
    }
}
