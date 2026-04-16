using AnarchyChess.Ai.BitForeverRules;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class BitMoveGeneratorTests
{
    private readonly BitMoveGenerator _generator = new();

    private readonly int PieceCount = Enum.GetValues<PieceType>().Length;

    [Fact]
    public void Generate_returns_expected_moves_for_white()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new AlgebraicPoint("f5")] = PieceFactory.White(PieceType.King),
                [new AlgebraicPoint("d2")] = PieceFactory.White(PieceType.Pawn, hasMoved: false),
                [new AlgebraicPoint("c2")] = new Piece(PieceType.TraitorRook, Color: null),

                [new AlgebraicPoint("b4")] = PieceFactory.Black(PieceType.King),
            },
            isWhiteToMove: true
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        HashSet<byte> expectedDestinations =
        [
            .. new AlgebraicPoint[]
            {
                // king
                new("e4"),
                new("e5"),
                new("e6"),
                new("f4"),
                new("f6"),
                new("g4"),
                new("g5"),
                new("g6"),
                // pawn
                new("d3"),
                new("d4"),
                new("d5"),
                // traitor rook
                new("c1"),
                new("c3"),
                new("c4"),
                new("c5"),
                new("c6"),
                new("c7"),
                new("c8"),
                new("c9"),
                new("c10"),
                new("b2"),
                new("a2"),
            }.Select(p => p.AsIdx()),
        ];

        moveCount.Should().Be(expectedDestinations.Count);
        foreach (var move in moves[..moveCount])
        {
            expectedDestinations.Should().Contain(move.To);
        }
    }

    [Fact]
    public void Generate_returns_expected_moves_for_black()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new AlgebraicPoint("d9")] = PieceFactory.Black(PieceType.Pawn, hasMoved: false),
                [new AlgebraicPoint("c9")] = new Piece(PieceType.TraitorRook, Color: null),

                [new AlgebraicPoint("f2")] = PieceFactory.White(PieceType.King),
            },
            isWhiteToMove: false
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        HashSet<byte> expectedDestinations =
        [
            .. new AlgebraicPoint[]
            {
                // pawn
                new("d8"),
                new("d7"),
                new("d6"),
                // traitor rook
                new("c10"),
                new("c8"),
                new("c7"),
                new("c6"),
                new("c5"),
                new("c4"),
                new("c3"),
                new("c2"),
                new("c1"),
                new("b9"),
                new("a9"),
            }.Select(p => p.AsIdx()),
        ];

        moveCount.Should().Be(expectedDestinations.Count);
        foreach (var move in moves[..moveCount])
        {
            expectedDestinations.Should().Contain(move.To);
        }
    }

    [Theory]
    [InlineData(8, 5, true)]
    [InlineData(8, 4, false)]
    public void Generate_passes_depth_and_max_depth(int maxDepth, int depth, bool generateThrows)
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.White(PieceType.Pawn, hasMoved: true),
                [new("e1")] = PieceFactory.White(PieceType.King),
                [new("f7")] = PieceFactory.Black(PieceType.Rook),
            }
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;
        _generator.Generate(board, moves, ref moveCount, depth: depth, maxDepth: maxDepth);

        if (generateThrows)
        {
            // king + 1 pawn + stun + throw
            moveCount.Should().Be(7);
        }
        else
        {
            // king + 1 pawn
            moveCount.Should().Be(5);
        }
    }

    [Fact]
    public void Generate_applies_forever_rules()
    {
        PrevMoveState prevMove = new(
            From: 0,
            To: BitOmnipotentPawnRule.WhiteSquare,
            Piece: new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.Black },
            CaptureMask: BitOmnipotentPawnRule.WhiteSquareMask,
            SpecialMoveType: SpecialMoveType.None
        );
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>(),
            prevMoveState: prevMove
        );

        Span<BitMove> moves = stackalloc BitMove[10];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        moveCount.Should().Be(1);
        moves[0]
            .Should()
            .BeEquivalentTo(
                new BitMove
                {
                    From = BitOmnipotentPawnRule.WhiteSquare,
                    To = BitOmnipotentPawnRule.WhiteSquare,
                    Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
                    CapturesMask = BitOmnipotentPawnRule.WhiteSquareMask,
                    SpecialMoveType = SpecialMoveType.OmnipotentPawnSpawn,
                }
            );
    }

    [Fact]
    public void Generate_returns_only_highest_priority_moves()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new("e5")] = PieceFactory.White(PieceType.Bishop),
                [new("h2")] = PieceFactory.White(PieceType.UnderagePawn),
            },
            isWhiteToMove: true
        );

        Span<BitMove> moves = stackalloc BitMove[256];
        int moveCount = 0;

        _generator.Generate(board, moves, ref moveCount);

        foreach (var move in moves[..moveCount])
        {
            move.ForcedMovePriority.Should().Be(ForcedMovePriority.UnderagePawn);
        }
    }
}
