using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class TranspositionTableTests
{
    private static int PackedMove(byte from, byte to) =>
        new BitMove
        {
            From = from,
            To = to,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        }.Pack();

    [Fact]
    public void TryProbe_returns_false_when_empty()
    {
        TranspositionTable tt = new(1);

        bool result = tt.TryProbe(12345UL, depth: 4, alpha: -1000, beta: 1000, out _, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryProbe_returns_false_when_key_does_not_match()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 4,
            score: 100,
            NodeType.Exact,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(99999UL, depth: 4, alpha: -1000, beta: 1000, out _, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryProbe_returns_false_when_stored_depth_is_less_than_requested()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 2,
            score: 100,
            NodeType.Exact,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(12345UL, depth: 4, alpha: -1000, beta: 1000, out _, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryProbe_returns_best_move_even_when_depth_is_insufficient()
    {
        TranspositionTable tt = new(1);
        int bestMove = PackedMove(10, 20);
        tt.Store(12345UL, depth: 2, score: 100, NodeType.Exact, bestMove: bestMove);

        tt.TryProbe(12345UL, depth: 4, alpha: -1000, beta: 1000, out _, out int returnedBestMove);

        returnedBestMove.Should().Be(bestMove);
    }

    [Fact]
    public void TryProbe_returns_true_and_score_for_exact_node()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 4,
            score: 150,
            NodeType.Exact,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(
            12345UL,
            depth: 4,
            alpha: -1000,
            beta: 1000,
            out int score,
            out _
        );

        result.Should().BeTrue();
        score.Should().Be(150);
    }

    [Fact]
    public void TryProbe_returns_true_for_lower_bound_when_score_exceeds_beta()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 4,
            score: 200,
            NodeType.LowerBound,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(12345UL, depth: 4, alpha: -1000, beta: 150, out int score, out _);

        result.Should().BeTrue();
        score.Should().Be(200);
    }

    [Fact]
    public void TryProbe_returns_false_for_lower_bound_when_score_does_not_exceed_beta()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 4,
            score: 100,
            NodeType.LowerBound,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(12345UL, depth: 4, alpha: -1000, beta: 150, out _, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryProbe_returns_true_for_upper_bound_when_score_is_below_alpha()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 4,
            score: 50,
            NodeType.UpperBound,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(12345UL, depth: 4, alpha: 100, beta: 1000, out int score, out _);

        result.Should().BeTrue();
        score.Should().Be(50);
    }

    [Fact]
    public void TryProbe_returns_false_for_upper_bound_when_score_is_not_below_alpha()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 4,
            score: 150,
            NodeType.UpperBound,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(12345UL, depth: 4, alpha: 100, beta: 1000, out _, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void Store_overwrites_entry_with_greater_or_equal_depth()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 3,
            score: 100,
            NodeType.Exact,
            bestMove: PackedMove(from: 0, to: 1)
        );
        tt.Store(
            12345UL,
            depth: 5,
            score: 200,
            NodeType.Exact,
            bestMove: PackedMove(from: 2, to: 3)
        );

        tt.TryProbe(12345UL, depth: 5, alpha: -1000, beta: 1000, out int score, out _);

        score.Should().Be(200);
    }

    [Fact]
    public void Store_does_not_overwrite_entry_with_lesser_depth()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 5,
            score: 200,
            NodeType.Exact,
            bestMove: PackedMove(from: 0, to: 1)
        );
        tt.Store(12345UL, depth: 3, score: 100, NodeType.Exact, bestMove: PackedMove(2, 3));

        tt.TryProbe(12345UL, depth: 5, alpha: -1000, beta: 1000, out int score, out _);

        score.Should().Be(200);
    }

    [Fact]
    public void TryProbe_returns_true_for_exact_node_at_greater_stored_depth()
    {
        TranspositionTable tt = new(1);
        tt.Store(
            12345UL,
            depth: 6,
            score: 150,
            NodeType.Exact,
            bestMove: PackedMove(from: 0, to: 1)
        );

        bool result = tt.TryProbe(
            12345UL,
            depth: 4,
            alpha: -1000,
            beta: 1000,
            out int score,
            out _
        );

        result.Should().BeTrue();
        score.Should().Be(150);
    }
}
