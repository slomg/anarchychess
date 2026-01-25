using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.SanNotation;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameCoreTests
{
    private readonly GameCore _core;

    private readonly IMoveEncoder _moveEncoderMock = Substitute.For<IMoveEncoder>();

    public GameCoreTests()
    {
        _core = new(
            Substitute.For<ILogger<GameCore>>(),
            Substitute.For<IFenEncoder>(),
            Substitute.For<IPlayableMoveProvider>(),
            Substitute.For<ISanCalculator>(),
            Substitute.For<IDrawEvaulator>(),
            Substitute.For<IGameResultDescriber>(),
            _moveEncoderMock
        );
    }

    [Fact]
    public void EncodeLegalMoves_encodes_legal_moves_from_state()
    {
        var movePaths = new MovePathFaker().Generate(3);
        var expectedMoveKeys = movePaths.Select(x => x.MoveKey);
        CompressedMoves compressedMoves = "compressedmoves";
        GameCoreState state = new()
        {
            LegalMoves = new(new Dictionary<MoveKey, Move>(), movePaths),
        };

        _moveEncoderMock
            .EncodeMoves(
                ArgEx.FluentAssert<List<MovePath>>(x =>
                    x?.Select(x => x.MoveKey).Should().BeEquivalentTo(expectedMoveKeys)
                )
            )
            .Returns(compressedMoves);

        var result = _core.EncodeLegalMoves(state);

        result.Should().BeEquivalentTo(compressedMoves);
    }
}
