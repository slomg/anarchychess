using AnarchyChess.Ai.Evaluation;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.EvaluationTests;

public class KingEndgameActivityEvaluatorTests
{
    [Fact]
    public void Evaluate_returns_zero_when_endgame_factor_too_low()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d5")] = PieceFactory.White(PieceType.King),
            [new("i5")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 0.1f
        );

        evaluation.WhiteScore.Should().Be(0);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_center_proximity()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("d5")] = PieceFactory.White(PieceType.King),
            [new("i5")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        evaluation.WhiteScore.Should().Be(KingEndgameActivityEvaluator.CenterProximityBonus - 2);
        evaluation.BlackScore.Should().Be(KingEndgameActivityEvaluator.CenterProximityBonus - 3);
    }

    [Fact]
    public void Evaluate_scores_white_king_proximity_to_own_passed_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f5")] = PieceFactory.White(PieceType.King),
            [new("c4")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        int expected = KingEndgameActivityEvaluator.OwnPassedPawnProximityBonus - 3;
        expected += KingEndgameActivityEvaluator.CenterProximityBonus;
        evaluation.WhiteScore.Should().Be(expected);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_black_king_proximity_to_own_passed_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f5")] = PieceFactory.Black(PieceType.King),
            [new("g7")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        int expected = KingEndgameActivityEvaluator.OwnPassedPawnProximityBonus - 2;
        expected += KingEndgameActivityEvaluator.CenterProximityBonus;
        evaluation.BlackScore.Should().Be(expected);
        evaluation.WhiteScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_white_proximity_to_enemy_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f5")] = PieceFactory.White(PieceType.King),
            [new("g7")] = PieceFactory.White(PieceType.Pawn),
            [new("g8")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        int expected = KingEndgameActivityEvaluator.CenterProximityBonus;
        expected += KingEndgameActivityEvaluator.EnemyPawnProximityBonus - 3;
        evaluation.WhiteScore.Should().Be(expected);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_scores_black_proximity_to_enemy_pawn()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f5")] = PieceFactory.Black(PieceType.King),
            [new("g7")] = PieceFactory.Black(PieceType.Pawn),
            [new("g6")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        int expected = KingEndgameActivityEvaluator.CenterProximityBonus;
        expected += KingEndgameActivityEvaluator.EnemyPawnProximityBonus - 1;
        evaluation.BlackScore.Should().Be(expected);
        evaluation.WhiteScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_only_scores_enemy_passed_pawns_proximity_if_any_for_white()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f5")] = PieceFactory.White(PieceType.King),
            [new("g7")] = PieceFactory.White(PieceType.Pawn),
            [new("g8")] = PieceFactory.Black(PieceType.Pawn),

            [new("a5")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        int expected = KingEndgameActivityEvaluator.CenterProximityBonus;
        expected += KingEndgameActivityEvaluator.EnemyPawnProximityBonus - 5;
        evaluation.WhiteScore.Should().Be(expected);
        evaluation.BlackScore.Should().Be(0);
    }

    [Fact]
    public void Evaluate_only_scores_enemy_passed_pawns_proximity_if_any_for_black()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("f5")] = PieceFactory.Black(PieceType.King),
            [new("g7")] = PieceFactory.Black(PieceType.Pawn),
            [new("g6")] = PieceFactory.White(PieceType.Pawn),

            [new("a5")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        EvaluationResult evaluation = KingEndgameActivityEvaluator.Evaluate(
            board,
            endgameFactor: 1f
        );

        int expected = KingEndgameActivityEvaluator.CenterProximityBonus;
        expected += KingEndgameActivityEvaluator.EnemyPawnProximityBonus - 5;
        evaluation.BlackScore.Should().Be(expected);
        evaluation.WhiteScore.Should().Be(0);
    }
}
