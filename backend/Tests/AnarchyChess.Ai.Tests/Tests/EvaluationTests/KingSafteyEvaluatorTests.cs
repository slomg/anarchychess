using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class KingSafetyEvaluatorTests
{
    private readonly KingSafetyEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_returns_zero_on_empty_board()
    {
        BitBoard board = new();

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_returns_zero_when_too_deep_to_endgame()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),

            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("f2")] = PieceFactory.White(PieceType.Pawn),
            [new("g2")] = PieceFactory.White(PieceType.Pawn),

            [new("a10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
            [new("f10")] = PieceFactory.Black(PieceType.King, hasMoved: false),
            [new("j10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),

            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(
            board,
            endgameFactor: KingSafetyEvaluator.EndgameFactorThreshold
        );

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
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

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(3 * KingSafetyEvaluator.PawnProtectionValue);
        blackScore.Should().Be(0);
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

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(0);
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

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore
            .Should()
            .Be(
                (2 * KingSafetyEvaluator.PawnProtectionValue)
                    - KingSafetyEvaluator.CenterStuckKingPenalty
            );
        blackScore.Should().Be(0);
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

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore
            .Should()
            .Be(2 * KingSafetyEvaluator.PawnProtectionValue * KingSafetyEvaluator.EdgeAmplifier);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_penalizes_center_stuck_king_without_castling_rights()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(-KingSafetyEvaluator.CenterStuckKingPenalty);
        blackScore.Should().Be(0);
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

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(-KingSafetyEvaluator.SemiStuckKingPenalty);
        blackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_evaluates_black_king_safety()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
            [new("f10")] = PieceFactory.Black(PieceType.King, hasMoved: false),
            [new("j10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),

            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor: 0);

        whiteScore.Should().Be(0);
        blackScore.Should().Be(2 * KingSafetyEvaluator.PawnProtectionValue);
    }

    [Fact]
    public void Evaluate_scales_king_safety_by_endgame_factor()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            [new("e2")] = PieceFactory.White(PieceType.Pawn),
            [new("f2")] = PieceFactory.White(PieceType.Pawn),
            [new("g2")] = PieceFactory.White(PieceType.Pawn),

            [new("a10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),
            [new("f10")] = PieceFactory.Black(PieceType.King, hasMoved: false),
            [new("j10")] = PieceFactory.Black(PieceType.Rook, hasMoved: false),

            [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            [new("f9")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float endgameFactor = 0.5f;

        int whiteRawSafety = 3 * KingSafetyEvaluator.PawnProtectionValue;
        int whiteExpected = (int)(whiteRawSafety * endgameFactor);

        int blackRawSafety = 2 * KingSafetyEvaluator.PawnProtectionValue;
        int blackExpected = (int)(blackRawSafety * endgameFactor);

        (int whiteScore, int blackScore) = _evaluator.Evaluate(board, endgameFactor);

        whiteScore.Should().Be(whiteExpected);
        blackScore.Should().Be(blackExpected);
    }
}
