using System.Text;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests.SanNotation;

public class ThrowNotatorTests
{
    private readonly ThrowNotator _notator = new(new PieceLetterMap());

    [Fact]
    public void HandlesMoveType_is_throw() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.Throw);

    [Fact]
    public void Notate_uses_throw_notation()
    {
        Move move = new(new("a1"), new("f1"), PieceFactory.White(PieceType.Pawn));

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("a1->f1");
    }

    [Fact]
    public void Notate_notates_stuns()
    {
        Move move = new(
            new("b2"),
            new("c4"),
            PieceFactory.White(PieceType.Pawn),
            stuns: [new MoveStun(new("c4"), PieceFactory.Black(), StunForTurns: 1)]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("b2->c4*");
    }
}
