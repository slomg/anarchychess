using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class KingSafetyEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = BitBoard.FromPieces([]);
        KingSafetyEvaluator.Evaluate(board).Should().Be(0);
    }

    [Fact]
    public void Evaluate_rewards_pawn_protection_around_white_king()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),

            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("f2")] = PieceFactory.White(PieceType.Pawn),
            [new("g2")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int expectedProtection = 3 * KingSafetyEvaluator.PawnProtectionValue;
        KingSafetyEvaluator.Evaluate(board).Should().Be(expectedProtection);
    }

    [Fact]
    public void Evaluate_ignores_pawns_not_adjacent_to_king()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            [new("a3")] = PieceFactory.White(PieceType.Pawn),
            [new("j3")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        KingSafetyEvaluator.Evaluate(board).Should().Be(0);
    }

    [Fact]
    public void Evaluate_applies_penalty_for_king_without_rooks()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("f2")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int protection = 2 * KingSafetyEvaluator.PawnProtectionValue;
        int expected = protection - KingSafetyEvaluator.CenterStuckKingPenalty;
        KingSafetyEvaluator.Evaluate(board).Should().Be(expected);
    }

    [Fact]
    public void Evaluate_applies_edge_amplifier_for_noncentral_king()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.King),
            [new("a2")] = PieceFactory.White(PieceType.Pawn),
            [new("b2")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int expectedProtection =
            2 * KingSafetyEvaluator.PawnProtectionValue * KingSafetyEvaluator.EdgeAmplifier;
        KingSafetyEvaluator.Evaluate(board).Should().Be(expectedProtection);
    }

    [Fact]
    public void Evaluate_penalizes_center_stuck_king_without_castling_rights()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        KingSafetyEvaluator
            .Evaluate(board)
            .Should()
            .Be(-KingSafetyEvaluator.CenterStuckKingPenalty);
    }

    [Fact]
    public void Evaluate_applies_semi_stuck_penalty_if_only_one_castle_blocked()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
            [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        KingSafetyEvaluator.Evaluate(board).Should().Be(-KingSafetyEvaluator.SemiStuckKingPenalty);
    }

    [Fact]
    public void Evaluate_flips_sign_when_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
            [new("f10")] = PieceFactory.Black(PieceType.King, hasMoved: false),
            [new("j10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),

            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        int blackScore = 2 * KingSafetyEvaluator.PawnProtectionValue * 1; // central file amplifier 1
        KingSafetyEvaluator.Evaluate(board).Should().Be(blackScore);
    }
}
